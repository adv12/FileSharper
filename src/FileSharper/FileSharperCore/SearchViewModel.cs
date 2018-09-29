// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using FileSharperCore.Util;

namespace FileSharperCore
{
    public class SearchViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public SharperEngine Engine { get; private set; }

        public int MaxResults { get; private set; }

        public int MaxExceptions { get; private set; }

        public DataTable SearchResults { get; } = new DataTable();

        public ObservableCollection<ExceptionInfo> ExceptionInfos { get; } = new ObservableCollection<ExceptionInfo>();

        private DispatcherTimer m_Timer;

        private List<FileProgressInfo> m_accumulatedFileProgressInfos = new List<FileProgressInfo>();

        private int m_ExceptionCount = 0;

        public int ExceptionCount
        {
            get => m_ExceptionCount;
            private set
            {
                SetField(ref m_ExceptionCount, value);
                OnPropertyChanged(nameof(ExceptionsHeader));
            }
        }

        public string ExceptionsHeader
        {
            get
            {
                if (m_ExceptionCount == 0)
                {
                    return "Exceptions";
                }
                return $"Exceptions ({ExceptionCount})";
            }
        }

        private int m_TestedCount = 0;
        public int TestedCount
        {
            get => m_TestedCount;
            private set
            {
                SetField(ref m_TestedCount, value);
            }
        }

        private int m_MatchedCount = 0;
        public int MatchedCount
        {
            get => m_MatchedCount;
            private set
            {
                SetField(ref m_MatchedCount, value);
            }
        }

        private string m_LatestFileSourceStatusText = null;
        private string m_FileSourceStatusText = null;
        public string FileSourceStatusText
        {
            get => m_FileSourceStatusText;
            set => SetField(ref m_FileSourceStatusText, value);
        }

        private string m_StatusText = null;
        public string StatusText
        {
            get => m_StatusText;
            private set => SetField(ref m_StatusText, value);
        }

        private string m_ExceptionText = string.Empty;
        public string ExceptionText
        {
            get => m_ExceptionText;
            set => SetField(ref m_ExceptionText, value);
        }

        public IProgress<string> FileSourceProgress { get; private set; }

        public IProgress<IEnumerable<FileProgressInfo>> TestedProgress { get; private set; }

        public IProgress<IEnumerable<FileProgressInfo>> MatchedProgress { get; private set; }

        public IProgress<IEnumerable<ExceptionInfo>> ExceptionProgress { get; private set; }

        public IProgress<bool> CompleteProgress { get; private set; }

        public string[] ColumnHeaders
        {
            get
            {
                List<string> headers = new List<string>();
                headers.Add("Filename");
                headers.Add("Path");
                return HeaderUtil.GetUniqueHeaders(headers, Engine.Condition, Engine.FieldSources);
            }
        }

        public string[] BindingColumnHeaders
        {
            get
            {
                List<string> headers = new List<string>();
                headers.Add("Filename");
                headers.Add("Path");
                return HeaderUtil.GetUniqueHeaders(headers, Engine.Condition, Engine.FieldSources, true);
            }
        }

        public CancellationTokenSource TokenSource { get; private set; }

        public bool StopRequested
        {
            get
            {
                return Engine.StopRequested;
            }
        }

        public ICommand CopyPathCommand { get; }

        public ICommand CopyFileCommand { get; }

        public ICommand OpenFileCommand { get; }

        public ICommand OpenContainingFolderCommand { get; }

        public ICommand OpenContainingFolderCommandPromptCommand { get; }

        public SearchViewModel(SharperEngine engine, int maxResults, int maxExceptions)
        {
            m_Timer = new DispatcherTimer();
            m_Timer.Interval = TimeSpan.FromSeconds(0.1);
            m_Timer.Tick += TimerTick;
            Engine = engine;
            MaxResults = maxResults;
            MaxExceptions = maxExceptions;
            ExceptionCount = 0;
            TokenSource = new CancellationTokenSource();
            CopyPathCommand = new PathCopier(this);
            CopyFileCommand = new FileCopier(this);
            OpenFileCommand = new FileOpener(this);
            OpenContainingFolderCommand = new ContainingFolderOpener(this);
            OpenContainingFolderCommandPromptCommand = new ContainingFolderCommandPromptOpener(this);

            foreach (string columnHeader in BindingColumnHeaders)
            {
                DataColumn column = new DataColumn(columnHeader);
                SearchResults.Columns.Add(column);
            }
            FileSourceProgress = new Progress<string>(message =>
            {
                m_LatestFileSourceStatusText = message;
            });
            TestedProgress = new Progress<IEnumerable<FileProgressInfo>>(infos =>
            {
                TestedCount += infos.Count();
            });
            MatchedProgress = new Progress<IEnumerable<FileProgressInfo>>(infos =>
            {
                int count = infos.Count();
                if (MatchedCount < maxResults)
                {
                    int numToTake = Math.Min(maxResults - MatchedCount, count);
                    m_accumulatedFileProgressInfos.AddRange(infos.Take(numToTake));
                }
                MatchedCount += count;
            });
            ExceptionProgress = new Progress<IEnumerable<ExceptionInfo>>(infos =>
            {
                foreach (ExceptionInfo info in infos)
                {
                    if (ExceptionCount < MaxExceptions)
                    {
                        StringBuilder sb = new StringBuilder(ExceptionText);
                        if (ExceptionCount > 0)
                        {
                            sb.AppendLine();
                            sb.AppendLine();
                        }
                        string name = null;
                        try
                        {
                            name = info.File?.Name;
                        }
                        catch (Exception ex) // particularly PathTooLongException
                        {
                            
                        }
                        sb.AppendFormat("Exception on file {0}: {1}", name ?? string.Empty, info.Exception.ToString());
                        ExceptionText = sb.ToString();
                        ExceptionCount++;
                    }
                    else
                    {
                        break;
                    }
                }

            });
            CompleteProgress = new Progress<bool>(x =>
            {
                m_Timer?.Stop();
                TimerTick(m_Timer, EventArgs.Empty);
            });
        }

        private void TimerTick(object sender, EventArgs e)
        {
            StatusText = "Matched " + MatchedCount + " of " + TestedCount;
            FileSourceStatusText = m_LatestFileSourceStatusText;
            foreach (FileProgressInfo info in m_accumulatedFileProgressInfos)
            {
                try
                {
                    string[] values = info.Values;
                    string[] result = new string[values.Length + 2];
                    result[0] = info.File.Name;
                    result[1] = info.File.DirectoryName;
                    Array.Copy(values, 0, result, 2, values.Length);
                    DataRow dataRow = SearchResults.NewRow();
                    for (int i = 0; i < result.Length; i++)
                    {
                        dataRow.SetField(i, result[i]);
                    }
                    SearchResults.Rows.Add(dataRow);
                }
                catch (Exception ex)
                {
                    Engine.RunInfo.ExceptionInfos.Enqueue(new ExceptionInfo(ex, info.File));
                }
            }
            m_accumulatedFileProgressInfos.Clear();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Search()
        {
            Engine.Run(TokenSource.Token, FileSourceProgress, TestedProgress, MatchedProgress, ExceptionProgress, CompleteProgress, m_Timer.Interval);
        }

        public Task SearchAsync()
        {
            m_Timer?.Start();
            return Task.Run((Action)(() => {
                Search();
            }));
        }

        public void RequestStop()
        {
            Engine.RequestStop();
            OnPropertyChanged(nameof(StopRequested));
        }

        public void Cancel()
        {
            TokenSource.Cancel();
        }

        public class PathCopier : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchViewModel ViewModel { get; private set; }

            public PathCopier(SearchViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;// parameter != null;
            }

            public void Execute(object parameter)
            {
                System.Collections.IList rowViews = (System.Collections.IList)parameter;
                if (rowViews != null)
                {
                    StringBuilder sb = new StringBuilder();
                    bool first = true;
                    foreach (DataRowView rowView in rowViews)
                    {
                        DataRow row = rowView.Row;
                        if (!first)
                        {
                            sb.AppendLine();
                        }
                        sb.Append(Path.Combine((string)row[1], (string)row[0]));
                        first = false;
                    }
                    System.Windows.Clipboard.SetText(sb.ToString());
                }
            }
        }

        public class FileOpener : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchViewModel ViewModel { get; private set; }

            public FileOpener(SearchViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;// parameter != null;
            }

            public void Execute(object parameter)
            {
                System.Collections.IList rowViews = (System.Collections.IList)parameter;
                if (rowViews != null)
                {
                    foreach (DataRowView rowView in rowViews)
                    {
                        DataRow row = rowView.Row;
                        try
                        {
                            Process.Start(Path.Combine((string)row[1], (string)row[0]));
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        public class ContainingFolderOpener : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchViewModel ViewModel { get; private set; }

            public ContainingFolderOpener(SearchViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;// parameter != null;
            }

            public void Execute(object parameter)
            {
                System.Collections.IList rowViews = (System.Collections.IList)parameter;
                if (rowViews != null)
                {
                    HashSet<string> openedFolders = new HashSet<string>();
                    foreach (DataRowView rowView in rowViews)
                    {
                        DataRow row = rowView.Row;
                        try
                        {
                            string dirName = (string)row[1];
                            if (!openedFolders.Contains(dirName))
                            {
                                openedFolders.Add(dirName);
                                Process.Start(dirName);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        public class ContainingFolderCommandPromptOpener : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchViewModel ViewModel { get; private set; }

            public ContainingFolderCommandPromptOpener(SearchViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                System.Collections.IList rowViews = (System.Collections.IList)parameter;
                if (rowViews != null)
                {
                    HashSet<string> openedFolders = new HashSet<string>();
                    foreach (DataRowView rowView in rowViews)
                    {
                        DataRow row = rowView.Row;
                        try
                        {
                            string dirName = (string)row[1];
                            if (!openedFolders.Contains(dirName))
                            {
                                openedFolders.Add(dirName);
                                ProcessStartInfo info = new ProcessStartInfo();
                                info.FileName = "cmd.exe";
                                info.WorkingDirectory = dirName;
                                info.UseShellExecute = false;
                                Process.Start(info);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        public class FileCopier : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchViewModel ViewModel { get; private set; }

            public FileCopier(SearchViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;// parameter != null;
            }

            public void Execute(object parameter)
            {
                System.Collections.IList rowViews = (System.Collections.IList)parameter;
                if (rowViews != null)
                {
                    StringCollection paths = new StringCollection();
                    foreach (DataRowView rowView in rowViews)
                    {
                        DataRow row = rowView.Row;
                        paths.Add(Path.Combine((string)row[1], (string)row[0]));
                    }
                    System.Windows.Clipboard.SetFileDropList(paths);
                }
            }
        }

    }
}
