// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSharperCore;
using Microsoft.Win32;

namespace FileSharper
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_SelectedIndex = 0;

        public bool m_ShowingHelp = false;

        public ObservableCollection<SearchDocument> SearchDocuments { get; } =
            new ObservableCollection<SearchDocument>();

        public int SelectedIndex {
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

        public bool ShowingHelp
        {
            get => m_ShowingHelp;
            set
            {
                if (m_ShowingHelp != value)
                {
                    m_ShowingHelp = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NewSearchCommand { get; private set; }
        public ICommand OpenSearchCommand { get; private set; }
        public ICommand CloseSearchCommand { get; private set; }
        public ICommand SaveSearchCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public ICommand AboutCommand { get; private set; }
        public ICommand HideAboutCommand { get; private set; }

        public MainViewModel()
        {
            SearchDocuments.Add(new SearchDocument(true));
            NewSearchCommand = new NewSearchMaker(this);
            OpenSearchCommand = new SearchOpener(this);
            CloseSearchCommand = new SearchCloser(this);
            SaveSearchCommand = new SearchSaver(this);
            ExitCommand = new ApplicationExiter(this);

            AboutCommand = new AboutBoxShower(this);
            HideAboutCommand = new AboutBoxHider(this);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                ViewModel.SearchDocuments.Add(new SearchDocument(true));
                ViewModel.SelectedIndex = ViewModel.SearchDocuments.Count - 1;
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
            ViewModel.ShowingHelp = true;
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
            ViewModel.ShowingHelp = false;
        }
    }
}
