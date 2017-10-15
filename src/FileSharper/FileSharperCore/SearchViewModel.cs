// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public int ExceptionCount { get; set; }

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

        public SearchViewModel(SharperEngine engine, int maxResults, int maxExceptions)
        {
            Engine = engine;
            MaxResults = maxResults;
            MaxExceptions = maxExceptions;
            TokenSource = new CancellationTokenSource();
            foreach (string columnHeader in ColumnHeaders)
            {
                DataColumn column = new DataColumn(columnHeader);
                SearchResults.Columns.Add(column);
            }
            TestedProgress = new Progress<FileProgressInfo>(info =>
            {

            });
            MatchedProgress = new Progress<FileProgressInfo>(info =>
            {
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

    }
}
