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
    public class FieldSourceNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_FieldSourceTypeName;
        private FieldSourcesNode m_Owner;
        private int m_Index;
        private IFieldSource m_FieldSourceInternal;

        [JsonProperty(Order = int.MinValue)]
        public string FieldSourceTypeName
        {
            get => m_FieldSourceTypeName;
            set
            {
                if (m_FieldSourceTypeName != value)
                {
                    m_FieldSourceTypeName = value;
                    if (value == null)
                    {
                        FieldSourceInternal = null;
                    }
                    else
                    {
                        FieldSourceInternal = FieldSourceCatalog.Instance.CreateFieldSource(m_FieldSourceTypeName);
                    }
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public FieldSourcesNode Owner
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
            get => m_Owner != null && m_Owner.FieldSourceNodes.Count - 1 == Index;
        }

        private IFieldSource FieldSourceInternal
        {
            get => m_FieldSourceInternal;
            set
            {
                if (m_FieldSourceInternal != value)
                {
                    IFieldSource old = m_FieldSourceInternal;
                    m_FieldSourceInternal = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Parameters));
                }
            }
        }

        public object Parameters
        {
            get
            {
                return m_FieldSourceInternal?.Parameters;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IFieldSource GetFieldSource()
        {
            return FieldSourceInternal;
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
