// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using FileSharperCore;
using Newtonsoft.Json;

namespace FileSharper
{
    public class SearchTemplateInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; }

        public string FileFullName
        {
            get {
                string dirPath = Stock ?
                    FileSharperSettings.StockTemplatesDirectoryPath:
                    FileSharperSettings.UserTemplatesDirectoryPath;
                return Path.Combine(dirPath, FileName);
            }
        }

        private string m_DisplayName;
        public string DisplayName
        {
            get => m_DisplayName;
            set => SetField(ref m_DisplayName, value);
        }

        private bool m_Stock;
        public bool Stock
        {
            get => m_Stock;
            set
            {
                SetField(ref m_Stock, value);
                OnPropertyChanged(nameof(User));
            }
        }

        [JsonIgnore]
        public bool User
        {
            get => !Stock;
        }

        private bool m_Hidden;
        public bool Hidden
        {
            get => m_Hidden;
            set => SetField(ref m_Hidden, value);
        }

        [JsonIgnore]
        public SearchDocument SampleInstance
        {
            get
            {
                return SearchDocument.FromFile(FileFullName, true);
            }
        }

        public SearchTemplateInfo(string fileName, string displayName, bool stock = false, bool hidden = false)
        {
            FileName = fileName;
            DisplayName = displayName;
            Stock = stock;
            Hidden = hidden;
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

    }
}
