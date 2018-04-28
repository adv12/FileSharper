// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FileSharperCore;
using Microsoft.Win32;

namespace FileSharper
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_SelectedScreenIndex = 0;

        private int m_SelectedIndex = 0;

        private bool m_AnyOpenFiles = false;

        private bool m_AnyRecentDocuments = false;

        private bool m_AnyTemplates = false;

        private bool m_ShowingAbout = false;

        private bool m_ShowingPathHelp = false;

        private bool m_ShowingMainUI = true;

        private bool m_ShowingSaveTemplateUI = false;

        private string m_SaveTemplateDisplayName = "";

        public FileSharperSettings Settings { get; }

        public ObservableCollection<SearchDocument> SearchDocuments { get; } =
            new ObservableCollection<SearchDocument>();

        public int SelectedScreenIndex
        {
            get => m_SelectedScreenIndex;
            set => SetField(ref m_SelectedScreenIndex, value);
        }

        public int SelectedIndex
        {
            get => m_SelectedIndex;
            set => SetField(ref m_SelectedIndex, value);
        }

        public bool AnyOpenFiles
        {
            get => m_AnyOpenFiles;
            private set => SetField(ref m_AnyOpenFiles, value);
        }

        public bool AnyRecentDocuments
        {
            get => m_AnyRecentDocuments;
            private set => SetField(ref m_AnyRecentDocuments, value);
        }

        public bool AnyTemplates
        {
            get => m_AnyTemplates;
            private set => SetField(ref m_AnyTemplates, value);
        }

        public bool ShowingSaveTemplateUI
        {
            get => m_ShowingSaveTemplateUI;
            set
            {
                if (m_ShowingSaveTemplateUI != value)
                {
                    m_ShowingSaveTemplateUI = value;
                    SaveTemplateDisplayName = string.Empty;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SearchDocumentsEnabled));
                }
            }
        }

        public string SaveTemplateDisplayName
        {
            get => m_SaveTemplateDisplayName;
            set => SetField(ref m_SaveTemplateDisplayName, value);
        }

        public bool SearchDocumentsEnabled => !ShowingSaveTemplateUI;

        public ICommand NewSearchCommand { get; private set; }
        public ICommand NewSearchFromTemplateCommand { get; private set; }
        public ICommand OpenSearchCommand { get; private set; }
        public ICommand OpenRecentCommand { get; private set; }
        public ICommand CloseSearchCommand { get; private set; }
        public ICommand SaveSearchCommand { get; private set; }
        public ICommand SaveTemplateCommand { get; private set; }
        public ICommand SaveDefaultTemplateCommand { get; private set; }
        public ICommand ResetDefaultTemplateCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public ICommand SetSelectedScreenIndexCommand { get; private set; }

        public ICommand NavigateCommand { get; private set; }

        public ICommand ShowSaveTemplateCommand { get; private set; }
        public ICommand HideSaveTemplateCommand { get; private set; }

        public MainViewModel()
        {
            Settings = FileSharperSettings.Load();

            Settings.RecentDocuments.CollectionChanged += RecentDocuments_CollectionChanged;
            AnyRecentDocuments = Settings.RecentDocuments.Count > 0;

            Settings.Templates.CollectionChanged += Templates_CollectionChanged;
            AnyTemplates = Settings.Templates.Count(t => !t.Hidden) > 0;

            SearchDocuments.CollectionChanged += SearchDocuments_CollectionChanged;

            AddNewSearch();
            NewSearchCommand = new NewSearchMaker(this);
            NewSearchFromTemplateCommand = new NewSearchFromTemplateMaker(this);
            OpenSearchCommand = new SearchOpener(this);
            OpenRecentCommand = new RecentSearchOpener(this);
            CloseSearchCommand = new SearchCloser(this);
            SaveSearchCommand = new SearchSaver(this);
            ShowSaveTemplateCommand = new ShowSaveTemplateSetter(this);
            SaveTemplateCommand = new SearchTemplateSaver(this);
            SaveDefaultTemplateCommand = new DefaultSearchTemplateSaver(this);
            ResetDefaultTemplateCommand = new DefaultSearchTemplateClearer(this);
            ExitCommand = new ApplicationExiter(this);

            SetSelectedScreenIndexCommand = new SelectedScreenIndexSetter(this);
            
            NavigateCommand = new LinkNavigator(this);
        }

        private void Templates_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AnyTemplates = Settings.Templates.Count(t => !t.Hidden) > 0;
        }

        private void RecentDocuments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AnyRecentDocuments = Settings.RecentDocuments.Count > 0;
        }

        private void SearchDocuments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            AnyOpenFiles = SearchDocuments.Count > 0;
        }

        public void AddNewSearch()
        {
            if (!AddNewSearchFromTemplate(FileSharperSettings.DefaultTemplatePath))
            {
                SearchDocument doc = new SearchDocument(true);
                SearchDocuments.Add(doc);
                SelectedIndex = SearchDocuments.Count - 1;
            }
        }

        public bool AddNewSearchFromTemplate(string templatePath)
        {
            SearchDocument doc = null;
            if (File.Exists(templatePath))
            {
                try
                {
                    doc = SearchDocument.FromFile(templatePath, true);
                }
                catch (Exception)
                {
                }
            }
            if (doc != null)
            {
                SearchDocuments.Add(doc);
                SelectedIndex = SearchDocuments.Count - 1;
                return true;
            }
            return false;
        }

        public void OpenFile(string filename)
        {
            SearchDocuments.Add(SearchDocument.FromFile(filename));
            SelectedIndex = SearchDocuments.Count - 1;
            Settings.AddRecentDocument(filename);
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

        public class SelectedScreenIndexSetter : ICommand
        {
            public event EventHandler CanExecuteChanged;


            public MainViewModel ViewModel
            {
                get; set;
            }

            public SelectedScreenIndexSetter(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                if (parameter is int)
                {
                    int index = (int)parameter;
                    ViewModel.SelectedScreenIndex = index;
                }
            }
        }

        public class NewSearchMaker : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public NewSearchMaker(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ViewModel.AddNewSearch();
            }
        }

        public class NewSearchFromTemplateMaker : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public NewSearchFromTemplateMaker(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                string templatePath = parameter as string;
                if (templatePath != null)
                {
                    ViewModel.AddNewSearchFromTemplate(templatePath);
                }
            }
        }

        public class SearchOpener : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public SearchOpener(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "FileSharper files (*.fsh)|*.fsh";
                bool? success = openFileDialog.ShowDialog();
                if (success.HasValue && success.Value)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        ViewModel.OpenFile(filename);
                    }
                }
            }
        }

        public class RecentSearchOpener : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public RecentSearchOpener(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                string filename = parameter as string;
                if (filename != null)
                {
                    if (File.Exists(filename))
                    {
                        ViewModel.OpenFile(filename);
                    }
                    else
                    {
                        ViewModel.Settings.RemoveRecentDocument(filename);
                    }
                }
            }
        }

        public class SearchCloser : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public SearchCloser(MainViewModel viewModel)
            {
                ViewModel = viewModel;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(ViewModel.AnyOpenFiles))
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool CanExecute(object parameter)
            {
                return ViewModel.AnyOpenFiles;
            }

            public void Execute(object parameter)
            {
                bool close = true;
                int idx = -1;
                if (parameter is SearchDocument)
                {
                    SearchDocument doc = (SearchDocument)parameter;
                    idx = ViewModel.SearchDocuments.IndexOf(doc);
                }
                else
                {
                    idx = ViewModel.SelectedIndex;
                    if (idx < 0 || idx >= ViewModel.SearchDocuments.Count)
                    {
                        close = false;
                    }
                }
                if (close)
                {
                    ViewModel.SearchDocuments.RemoveAt(idx);
                }
            }
        }

        public class SearchSaver : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public SearchSaver(MainViewModel viewModel)
            {
                ViewModel = viewModel;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(ViewModel.AnyOpenFiles))
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool CanExecute(object parameter)
            {
                return ViewModel.AnyOpenFiles;
            }

            public void Execute(object parameter)
            {
                bool saveAs = parameter != null && (bool)parameter;
                int idx = ViewModel.SelectedIndex;
                bool save = false;
                if (idx >= 0 && idx < ViewModel.SearchDocuments.Count)
                {
                    SearchDocument doc = ViewModel.SearchDocuments[idx];
                    string path = doc.FileName;
                    if (saveAs || string.IsNullOrEmpty(path))
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Filter = "FileSharper files (*.fsh)|*.fsh";
                        bool? result = sfd.ShowDialog();
                        if (result.HasValue && result.Value)
                        {
                            path = sfd.FileName;
                            save = true;
                        }
                    }
                    else
                    {
                        save = true;
                    }
                    if (save)
                    {
                        doc.Save(path);
                        doc.FileName = path;
                        ViewModel.Settings.AddRecentDocument(path);
                    }
                }
            }
        }

        public class SearchTemplateSaver : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public SearchTemplateSaver(MainViewModel viewModel)
            {
                ViewModel = viewModel;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(ViewModel.AnyOpenFiles))
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool CanExecute(object parameter)
            {
                return ViewModel.AnyOpenFiles;
            }

            public void Execute(object parameter)
            {
                int idx = ViewModel.SelectedIndex;
                if (idx >= 0 && idx < ViewModel.SearchDocuments.Count)
                {
                    string templateName = parameter as string;
                    if (!string.IsNullOrEmpty(templateName))
                    {
                        SearchDocument doc = ViewModel.SearchDocuments[idx];
                        if (doc != null)
                        {
                            ViewModel.Settings.AddTemplate(doc, templateName);
                        }
                    }
                }
                ViewModel.ShowingSaveTemplateUI = false;
            }
        }

        public class DefaultSearchTemplateSaver : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public DefaultSearchTemplateSaver(MainViewModel viewModel)
            {
                ViewModel = viewModel;
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(ViewModel.AnyOpenFiles))
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool CanExecute(object parameter)
            {
                return ViewModel.AnyOpenFiles;
            }

            public void Execute(object parameter)
            {
                int idx = ViewModel.SelectedIndex;
                if (idx >= 0 && idx < ViewModel.SearchDocuments.Count)
                {
                    MessageBoxResult result = MessageBoxResult.OK;
                    if (parameter is Window)
                    {
                        result = MessageBox.Show(parameter as Window,
                            "Save the current search as the template for new searches?", "Save Template?",
                            MessageBoxButton.OKCancel);
                        if (result == MessageBoxResult.OK)
                        {
                            SearchDocument doc = ViewModel.SearchDocuments[idx];
                            if (doc != null)
                            {
                                ViewModel.Settings.SetDefaultTemplate(doc);
                            }
                        }
                    }
                }
            }
        }

        public class DefaultSearchTemplateClearer : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public DefaultSearchTemplateClearer(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                MessageBoxResult result = MessageBoxResult.OK;
                if (parameter is Window)
                {
                    result = MessageBox.Show(parameter as Window,
                        "Reset the template for new searches to an empty search?", "Reset Template?",
                        MessageBoxButton.OKCancel);
                }
                if (result == MessageBoxResult.OK)
                {
                    ViewModel.Settings.ResetDefaultTemplate();
                }
            }
        }

        public class ApplicationExiter : ICommand
        {
            public event EventHandler CanExecuteChanged;


            public MainViewModel ViewModel
            {
                get; set;
            }

            public ApplicationExiter(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        public class LinkNavigator : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public LinkNavigator(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                string url = parameter as string;
                if (url != null)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(url);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open hyperlink {url}: {ex}");
                    }
                }
            }
        }

        public class ShowSaveTemplateSetter : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public ShowSaveTemplateSetter(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                bool? hide = parameter as bool?;
                if (hide.HasValue && hide.Value)
                {
                    ViewModel.ShowingSaveTemplateUI = false;
                }
                else
                {
                    ViewModel.ShowingSaveTemplateUI = true;
                }
            }
        }


    }
}
