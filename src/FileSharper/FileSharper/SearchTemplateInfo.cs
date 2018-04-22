// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace FileSharper
{
    public class SearchTemplateInfo
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public string FileName { get; }

        public string FileFullName => Path.Combine(FileSharperSettings.TemplatesDirectoryPath, FileName);

        private string m_DisplayName;
        public string DisplayName
        {
            get => m_DisplayName;
            set => SetField(ref m_DisplayName, value);
        }

        private bool m_Hidden;
        public bool Hidden
        {
            get => m_Hidden;
            set => SetField(ref m_Hidden, value);
        }

        public SearchTemplateInfo(string fileName, string displayName, bool hidden = false)
        {
            FileName = fileName;
            DisplayName = displayName;
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
