// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace FileSharperCore
{
    public class OutputNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_OutputTypeName;
        private OutputsNode m_Owner;
        private int m_Index;
        private IOutput m_OutputInternal;

        [JsonProperty(Order = int.MinValue)]
        public string OutputTypeName
        {
            get => m_OutputTypeName;
            set
            {
                if (m_OutputTypeName != value)
                {
                    m_OutputTypeName = value;
                    if (value == null)
                    {
                        OutputInternal = null;
                    }
                    else
                    {
                        OutputInternal = OutputCatalog.Instance.CreateOutput(m_OutputTypeName);
                    }
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public OutputsNode Owner
        {
            get => m_Owner;
            set
            {
                SetField(ref m_Owner, value);
                OnPropertyChanged(nameof(First));
                OnPropertyChanged(nameof(Last));
            }
        }

        [JsonIgnore]
        public int Index
        {
            get => m_Index;
            set
            {
                SetField(ref m_Index, value);
                OnPropertyChanged(nameof(First));
                OnPropertyChanged(nameof(Last));
            }
        }

        [JsonIgnore]
        public bool First
        {
            get => Index == 0;
        }

        [JsonIgnore]
        public bool Last
        {
            get => m_Owner != null && m_Owner.OutputNodes.Count - 1 == Index;
        }

        private IOutput OutputInternal
        {
            get => m_OutputInternal;
            set
            {
                if (m_OutputInternal != value)
                {
                    IOutput old = m_OutputInternal;
                    m_OutputInternal = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Parameters));
                }
            }
        }

        public object Parameters
        {
            get
            {
                return m_OutputInternal?.Parameters;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IOutput GetOutput()
        {
            return OutputInternal;
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
