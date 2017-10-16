// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileSharperCore
{
    public class SearchDocument : INotifyPropertyChanged
    {

        public static SearchDocument FromFile(string path)
        {
            string json = File.ReadAllText(path);
            SearchDocument doc = JsonConvert.DeserializeObject<SearchDocument>(json);
            doc.Loaded = true;
            doc.FileName = path;
            return doc;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string m_DisplayName = "New Search";
        [JsonIgnore]
        public string DisplayName {
            get => m_DisplayName;
            private set
            {
                SetField(ref m_DisplayName, value);
            }
        }

        private string m_FileName = null;
        [JsonIgnore]
        public string FileName
        {
            get => m_FileName;
            set
            {
                SetField(ref m_FileName, value);
                if (m_FileName != null)
                {
                    DisplayName = Path.GetFileName(m_FileName);
                }
            }
        }

        public FileSourceNode FileSourceNode { get; set; } = new FileSourceNode();
        public ConditionNode ConditionNode { get; set; } = new ConditionNode();
        public OutputsNode OutputsNode { get; set; } = new OutputsNode();
        public ProcessorsNode TestedProcessorsNode { get; set; } = new ProcessorsNode();
        public ProcessorsNode MatchedProcessorsNode { get; set; } = new ProcessorsNode();

        private SearchViewModel m_SearchViewModel = null;
        [JsonIgnore]
        public SearchViewModel SearchViewModel
        {
            get => m_SearchViewModel;
            set => SetField(ref m_SearchViewModel, value);
        }
        
        private int m_MaxResults = 200;
        public int MaxResults
        {
            get => m_MaxResults;
            set => SetField(ref m_MaxResults, value);
        }

        private int m_MaxExceptions = 20;
        public int MaxExceptions
        {
            get => m_MaxExceptions;
            set => SetField(ref m_MaxExceptions, value);
        }

        private bool m_Searching = false;
        public bool Searching
        {
            get => m_Searching;
            set => SetField(ref m_Searching, value);
        }

        private bool m_Loaded;
        [JsonIgnore]
        public bool Loaded
        {
            get => m_Loaded;
            set
            {
                SetField(ref m_Loaded, value);
                ConditionNode.Loaded = value;
            }
        }

        public SearchDocument()
        {
            SearchCommand = new SearchRunner(this);
            CancelCommand = new SearchCanceller(this);
        }

        public SearchDocument(bool addStarterNodes): this()
        {
            if (addStarterNodes)
            {
                OutputsNode.OutputNodes.Add(new OutputNode());
                TestedProcessorsNode.ProcessorNodes.Add(new ProcessorNode());
                MatchedProcessorsNode.ProcessorNodes.Add(new ProcessorNode());
            }
            Loaded = true;
        }

        [JsonIgnore]
        public ICommand SearchCommand { get; private set; }

        [JsonIgnore]
        public ICommand CancelCommand { get; private set; }

        public SharperEngine GetEngine()
        {
            return new SharperEngine(FileSourceNode.GetFileSource(),
                ConditionNode.BuildCondition(), OutputsNode.GetOutputs(),
                TestedProcessorsNode.GetProcessors(), MatchedProcessorsNode.GetProcessors());
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

        public void Save(string filename)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new StringEnumConverter());
            settings.Formatting = Formatting.Indented;
            string text = JsonConvert.SerializeObject(this, settings);
            File.WriteAllText(filename, text);
        }

        public class SearchRunner : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchDocument Document
            {
                get; set;
            }

            public SearchRunner(SearchDocument document)
            {
                Document = document;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public async void Execute(object parameter)
            {
                SharperEngine engine = Document.GetEngine();
                if (engine == null)
                {
                    return;
                }
                SearchViewModel searchViewModel = new SearchViewModel(engine, Document.MaxResults, Document.MaxExceptions);
                Document.SearchViewModel = searchViewModel;
                Document.Searching = true;
                try
                {
                    await searchViewModel.SearchAsync();
                }
                catch (OperationCanceledException)
                {

                }
                finally
                {
                    Document.Searching = false;
                }
            }
        }

        public class SearchCanceller : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public SearchDocument Document
            {
                get; set;
            }

            public SearchCanceller(SearchDocument document)
            {
                Document = document;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Document.SearchViewModel?.Cancel();
            }
        }

    }
}
