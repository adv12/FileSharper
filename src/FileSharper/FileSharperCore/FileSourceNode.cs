// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace FileSharperCore
{
    public class FileSourceNode : INotifyPropertyChanged
    {
        private string m_FileSourceTypeName;
        public event PropertyChangedEventHandler PropertyChanged;

        private IFileSource m_FileSourceInternal;

        [JsonProperty(Order = int.MinValue)]
        public string FileSourceTypeName
        {
            get => m_FileSourceTypeName;
            set
            {
                if (m_FileSourceTypeName != value)
                {
                    m_FileSourceTypeName = value;
                    if (value == null)
                    {
                        FileSourceInternal = null;
                    }
                    else
                    {
                        FileSourceInternal = FileSourceCatalog.Instance.CreateFileSource(m_FileSourceTypeName);
                    }
                    OnPropertyChanged();
                }
            }
        }

        private IFileSource FileSourceInternal
        {
            get => m_FileSourceInternal;
            set
            {
                if (m_FileSourceInternal != value)
                {
                    IFileSource old = m_FileSourceInternal;
                    m_FileSourceInternal = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Parameters));
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public string Description => m_FileSourceInternal?.Description;

        public object Parameters
        {
            get
            {
                return m_FileSourceInternal?.Parameters;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IFileSource GetFileSource()
        {
            return FileSourceInternal;
        }

    }
}
