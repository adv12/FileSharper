// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

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

        private int m_testedCount = 0;
        public int TestedCount
        {
            get => m_testedCount;
            private set
            {
                SetField(ref m_testedCount, value);
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private int m_matchedCount = 0;
        public int MatchedCount
        {
            get => m_matchedCount;
            private set
            {
                SetField(ref m_matchedCount, value);
                OnPropertyChanged(nameof(StatusText));
            }
        }

        public string StatusText
        {
            get => "Matched " + MatchedCount + " of " + TestedCount;
        }

        private string m_ExceptionText = string.Empty;
        public string ExceptionText
        {
            get => m_ExceptionText;
            set => SetField(ref m_ExceptionText, value);
        }

        public IProgress<FileProgressInfo> TestedProgress { get; private set; }

        public IProgress<FileProgressInfo> MatchedProgress { get; private set; }

        public IProgress<ExceptionInfo> ExceptionProgress { get; private set; }

        public string[] ColumnHeaders
        {
            get
            {
                List<string> headers = new List<string>();
                headers.Add("Filename");
                headers.Add("Path");
                if (Engine.Condition != null)
                {
                    headers.AddRange(Engine.Condition.ColumnHeaders);
                }
                if (Engine.Outputs != null)
                {
                    foreach (IOutput output in Engine.Outputs)
                    {
                        headers.AddRange(output.ColumnHeaders);
                    }
                }
                return headers.ToArray();
            }
        }

        public CancellationTokenSource TokenSource { get; private set; }

        public ICommand CopyPathCommand { get; }

        public SearchViewModel(SharperEngine engine, int maxResults, int maxExceptions)
        {
            Engine = engine;
            MaxResults = maxResults;
            MaxExceptions = maxExceptions;
            ExceptionCount = 0;
            TokenSource = new CancellationTokenSource();
            CopyPathCommand = new PathCopier(this);
            foreach (string columnHeader in ColumnHeaders)
            {
                DataColumn column = new DataColumn(columnHeader);
                SearchResults.Columns.Add(column);
            }
            TestedProgress = new Progress<FileProgressInfo>(info =>
            {
                TestedCount++;
            });
            MatchedProgress = new Progress<FileProgressInfo>(info =>
            {
                MatchedCount++;
                if (SearchResults.Rows.Count <= maxResults)
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
            });
            ExceptionProgress = new Progress<ExceptionInfo>(info =>
            {
                if (ExceptionCount <= MaxExceptions)
                {
                    StringBuilder sb = new StringBuilder(ExceptionText);
                    if (ExceptionCount > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    sb.AppendFormat("Exception on file {0}: {1}", info.File?.Name, info.Exception.ToString());
                    ExceptionText = sb.ToString();
                    ExceptionCount++;
                }
            });
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
            Engine.Run(TokenSource.Token, TestedProgress, MatchedProgress, ExceptionProgress, null);
        }

        public Task SearchAsync()
        {
            return Task.Run((Action)(() => {
                Search();
            }));
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

    }
}
