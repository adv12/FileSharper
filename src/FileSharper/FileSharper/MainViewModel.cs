// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections;
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

        public ICommand AcceptEulaCommand { get; private set; }
        public ICommand NewSearchCommand { get; private set; }
        public ICommand NewSearchFromTemplateCommand { get; private set; }
        public ICommand OpenSearchCommand { get; private set; }
        public ICommand OpenRecentCommand { get; private set; }
        public ICommand CloseSearchCommand { get; private set; }
        public ICommand SaveSearchCommand { get; private set; }
        public ICommand SaveTemplateCommand { get; private set; }
        public ICommand SaveDefaultTemplateCommand { get; private set; }
        public ICommand ResetDefaultTemplateCommand { get; private set; }
        public ICommand DeleteTemplatesCommand { get; private set; }
        public ICommand MoveTemplatesUpCommand { get; private set; }
        public ICommand MoveTemplatesDownCommand { get; private set; }
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

            AcceptEulaCommand = new MainViewModelCommand(this, p => { Settings.EulaAccepted = true; }, false, p => true);
            NewSearchCommand = new MainViewModelCommand(this, p => { AddNewSearch(); });
            NewSearchFromTemplateCommand = new MainViewModelCommand(this, NewSearchFromTemplate);
            OpenSearchCommand = new MainViewModelCommand(this, OpenSearch);
            OpenRecentCommand = new MainViewModelCommand(this, OpenRecentSearch);
            CloseSearchCommand = new MainViewModelCommand(this, CloseSearch, true);
            SaveSearchCommand = new MainViewModelCommand(this, SaveSearch, true);
            ShowSaveTemplateCommand = new MainViewModelCommand(this, SetShowSaveTemplates, true);
            SaveTemplateCommand = new MainViewModelCommand(this, SaveSearchTemplate, true);
            SaveDefaultTemplateCommand = new MainViewModelCommand(this, SaveDefaultSearchTemplate, true);
            ResetDefaultTemplateCommand = new MainViewModelCommand(this, ResetDefaultSearchTemplate);
            DeleteTemplatesCommand = new MainViewModelCommand(this, DeleteTemplates);
            MoveTemplatesUpCommand = new MainViewModelCommand(this, MoveTemplatesUp);
            MoveTemplatesDownCommand = new MainViewModelCommand(this, MoveTemplatesDown);
            ExitCommand = new MainViewModelCommand(this, p => { Application.Current.Shutdown(); });

            SetSelectedScreenIndexCommand = new MainViewModelCommand(this, SetSelectedScreenIndex, false, p => true );
            
            NavigateCommand = new MainViewModelCommand(this, Navigate);
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

        public void NewSearchFromTemplate(object parameter)
        {
            string templatePath = parameter as string;
            if (templatePath != null)
            {
                AddNewSearchFromTemplate(templatePath);
            }
        }

        public void SetSelectedScreenIndex(object parameter)
        {
            if (parameter is int)
            {
                int index = (int)parameter;
                SelectedScreenIndex = index;
            }
        }

        public void OpenSearch(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "FileSharper files (*.fsh)|*.fsh";
            bool? success = openFileDialog.ShowDialog();
            if (success.HasValue && success.Value)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    OpenFile(filename);
                }
            }
        }

        public void OpenRecentSearch(object parameter)
        {
            string filename = parameter as string;
            if (filename != null)
            {
                if (File.Exists(filename))
                {
                    OpenFile(filename);
                }
                else
                {
                    Settings.RemoveRecentDocument(filename);
                }
            }
        }

        public void CloseSearch(object parameter)
        {
            bool close = true;
            int idx = -1;
            if (parameter is SearchDocument)
            {
                SearchDocument doc = (SearchDocument)parameter;
                idx = SearchDocuments.IndexOf(doc);
            }
            else
            {
                idx = SelectedIndex;
                if (idx < 0 || idx >= SearchDocuments.Count)
                {
                    close = false;
                }
            }
            if (close)
            {
                SearchDocuments.RemoveAt(idx);
            }
        }

        public void SaveSearch(object parameter)
        {
            bool saveAs = parameter != null && (bool)parameter;
            int idx = SelectedIndex;
            bool save = false;
            if (idx >= 0 && idx < SearchDocuments.Count)
            {
                SearchDocument doc = SearchDocuments[idx];
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
                    Settings.AddRecentDocument(path);
                }
            }
        }

        public void SaveSearchTemplate(object parameter)
        {
            int idx = SelectedIndex;
            if (idx >= 0 && idx < SearchDocuments.Count)
            {
                string templateName = parameter as string;
                if (!string.IsNullOrEmpty(templateName))
                {
                    SearchDocument doc = SearchDocuments[idx];
                    if (doc != null)
                    {
                        Settings.AddTemplate(doc, templateName);
                    }
                }
            }
            ShowingSaveTemplateUI = false;
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

        public void SaveDefaultSearchTemplate(object parameter)
        {
            int idx = SelectedIndex;
            if (idx >= 0 && idx < SearchDocuments.Count)
            {
                MessageBoxResult result = MessageBoxResult.OK;
                if (parameter is Window)
                {
                    result = MessageBox.Show(parameter as Window,
                        "Save the current search as the template for new searches?", "Save Template?",
                        MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        SearchDocument doc = SearchDocuments[idx];
                        if (doc != null)
                        {
                            Settings.SetDefaultTemplate(doc);
                        }
                    }
                }
            }
        }

        public void ResetDefaultSearchTemplate(object parameter)
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
                Settings.ResetDefaultTemplate();
            }
        }

        public void Navigate(object parameter)
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

        public void SetShowSaveTemplates(object parameter)
        {
            bool? hide = parameter as bool?;
            if (hide.HasValue && hide.Value)
            {
                ShowingSaveTemplateUI = false;
            }
            else
            {
                ShowingSaveTemplateUI = true;
            }
        }

        public void DeleteTemplates(object parameter)
        {
            IList list = parameter as IList;
            if (list != null)
            {
                List<SearchTemplateInfo> templatesToRemove = list.OfType<SearchTemplateInfo>().ToList();
                foreach (SearchTemplateInfo template in templatesToRemove)
                {
                    if (!template.Stock)
                    {
                        Settings.Templates.Remove(template);
                        try
                        {
                            File.Delete(template.FileFullName);
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }

        public void MoveTemplatesUp(object parameter)
        {
            IList list = parameter as IList;
            if (list != null)
            {
                ObservableCollection<SearchTemplateInfo> templates = Settings.Templates;
                List<SearchTemplateInfo> templatesToMove = list.OfType<SearchTemplateInfo>()
                    .OrderBy(t => templates.IndexOf(t)).ToList();
                int lowestIndex = templates.Count - 1;
                foreach (SearchTemplateInfo template in templatesToMove)
                {
                    int index = templates.IndexOf(template);
                    if (index > -1 && index < lowestIndex)
                    {
                        lowestIndex = index;
                    }
                }
                int insertionIndex = lowestIndex == 0 ? 0 : lowestIndex - 1;
                for (int i = templatesToMove.Count - 1; i >= 0; i--)
                {
                    templates.Move(templates.IndexOf(templatesToMove[i]), insertionIndex);
                }
            }
        }

        public void MoveTemplatesDown(object parameter)
        {
            IList list = parameter as IList;
            if (list != null)
            {
                ObservableCollection<SearchTemplateInfo> templates = Settings.Templates;
                List<SearchTemplateInfo> templatesToMove = list.OfType<SearchTemplateInfo>()
                    .OrderBy(t => templates.IndexOf(t)).ToList();
                int highestIndex = -1;
                foreach (SearchTemplateInfo template in templatesToMove)
                {
                    int index = templates.IndexOf(template);
                    if (index > highestIndex)
                    {
                        highestIndex = index;
                    }
                }
                int insertionIndex = highestIndex + 1;
                if (insertionIndex >= templates.Count)
                {
                    insertionIndex = templates.Count - 1;
                }
                foreach (SearchTemplateInfo template in templatesToMove)
                {
                    templates.Move(templates.IndexOf(template), insertionIndex);
                }
            }
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
        
        public class MainViewModelCommand : ICommand
        {

            public delegate bool CanExecuteTester(object parameter);
            public delegate void CommandExecutor(object parameter);

            public event EventHandler CanExecuteChanged;

            public MainViewModel ViewModel
            {
                get; private set;
            }

            private CanExecuteTester m_CanExecuteTester;
            private CommandExecutor m_CommandExecutor;

            private bool m_RequiresOpenFile;

            public MainViewModelCommand(MainViewModel viewModel, CommandExecutor commandExecutor,
                bool requiresOpenFile = false, CanExecuteTester canExecuteTester = null)
            {
                ViewModel = viewModel;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.Settings.PropertyChanged += Settings_PropertyChanged;
                m_CommandExecutor = commandExecutor;
                m_CanExecuteTester = canExecuteTester;
                m_RequiresOpenFile = requiresOpenFile;
            }

            private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            public bool CanExecute(object parameter)
            {
                if (m_CanExecuteTester != null)
                {
                    return m_CanExecuteTester(parameter);
                }
                return ViewModel.Settings.EulaAccepted && (!m_RequiresOpenFile || ViewModel.AnyOpenFiles);
            }

            public void Execute(object parameter)
            {
                m_CommandExecutor?.Invoke(parameter);
            }
        }

    }
}
