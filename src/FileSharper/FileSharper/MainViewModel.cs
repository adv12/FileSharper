// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        private int m_SelectedIndex = 0;

        private bool m_ShowingAbout = false;

        private bool m_ShowingPathHelp = false;

        private bool m_ShowingMainUI = true;

        public ObservableCollection<SearchDocument> SearchDocuments { get; } =
            new ObservableCollection<SearchDocument>();

        public FileSharperSettings Settings { get; }

        public int SelectedIndex
        {
            get => m_SelectedIndex;
            set
            {
                if (m_SelectedIndex != value)
                {
                    m_SelectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowingAbout
        {
            get => m_ShowingAbout;
            set
            {
                if (m_ShowingAbout != value)
                {
                    m_ShowingAbout = value;
                    if (value)
                    {
                        ShowingPathHelp = false;
                        ShowingMainUI = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowingPathHelp
        {
            get => m_ShowingPathHelp;
            set
            {
                if (m_ShowingPathHelp != value)
                {
                    m_ShowingPathHelp = value;
                    if (value)
                    {
                        ShowingAbout = false;
                        ShowingMainUI = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowingMainUI
        {
            get => m_ShowingMainUI;
            set
            {
                if (m_ShowingMainUI != value)
                {
                    m_ShowingMainUI = value;
                    if (value)
                    {
                        ShowingAbout = false;
                        ShowingPathHelp = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NewSearchCommand { get; private set; }
        public ICommand OpenSearchCommand { get; private set; }
        public ICommand CloseSearchCommand { get; private set; }
        public ICommand SaveSearchCommand { get; private set; }
        public ICommand SaveTemplateCommand { get; private set; }
        public ICommand ResetTemplateCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public ICommand AboutCommand { get; private set; }
        public ICommand HideAboutCommand { get; private set; }

        public ICommand PathHelpCommand { get; private set; }
        public ICommand HidePathHelpCommand { get; private set; }

        public ICommand NavigateCommand { get; private set; }

        public MainViewModel()
        {
            Settings = FileSharperSettings.Load();

            AddNewSearch();
            NewSearchCommand = new NewSearchMaker(this);
            OpenSearchCommand = new SearchOpener(this);
            CloseSearchCommand = new SearchCloser(this);
            SaveSearchCommand = new SearchSaver(this);
            SaveTemplateCommand = new SearchTemplateSaver(this);
            ResetTemplateCommand = new SearchTemplateClearer(this);
            ExitCommand = new ApplicationExiter(this);

            AboutCommand = new AboutBoxShower(this);
            HideAboutCommand = new AboutBoxHider(this);

            PathHelpCommand = new PathHelpShower(this);
            HidePathHelpCommand = new PathHelpHider(this);

            NavigateCommand = new LinkNavigator(this);
        }

        public void AddNewSearch()
        {
            if (Settings?.NewSearchTemplate != null)
            {
                SearchDocuments.Add((SearchDocument)Settings.NewSearchTemplate.Clone());
            }
            else
            {
                SearchDocuments.Add(new SearchDocument(true));
            }
            SelectedIndex = SearchDocuments.Count - 1;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (!ShowingAbout && !ShowingPathHelp)
            {
                ShowingMainUI = true;
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
                openFileDialog.Filter = "FileSharper files (*.fsh)|*.fsh";
                bool? success = openFileDialog.ShowDialog();
                if (success.HasValue && success.Value)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        ViewModel.SearchDocuments.Add(SearchDocument.FromFile(openFileDialog.FileName));
                        ViewModel.SelectedIndex = ViewModel.SearchDocuments.Count - 1;
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
            }

            public bool CanExecute(object parameter)
            {
                return true;
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
            }

            public bool CanExecute(object parameter)
            {
                return true;
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
            }

            public bool CanExecute(object parameter)
            {
                return true;
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
                                ViewModel.Settings.NewSearchTemplate = doc.Clone() as SearchDocument;
                            }
                        }
                    }
                }
            }
        }

        public class SearchTemplateClearer : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public SearchTemplateClearer(MainViewModel viewModel)
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
                    ViewModel.Settings.NewSearchTemplate = null;
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

        public class AboutBoxShower : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public AboutBoxShower(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ViewModel.ShowingAbout = true;
            }
        }

        public class AboutBoxHider : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public AboutBoxHider(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ViewModel.ShowingAbout = false;
            }
        }

        public class PathHelpShower : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public PathHelpShower(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ViewModel.ShowingPathHelp = true;
            }
        }

        public class PathHelpHider : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; set;
            }

            public PathHelpHider(MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ViewModel.ShowingPathHelp = false;
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
    }
}
