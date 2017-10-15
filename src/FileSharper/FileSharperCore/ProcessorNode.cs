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
    public class ProcessorNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_ProcessorTypeName;
        private ProcessorsNode m_Owner;
        private int m_Index;

        private IProcessor m_ProcessorInternal;

        [JsonProperty(Order = int.MinValue)]
        public string ProcessorTypeName
        {
            get => m_ProcessorTypeName;
            set
            {
                if (m_ProcessorTypeName != value)
                {
                    m_ProcessorTypeName = value;
                    if (value == null)
                    {
                        ProcessorInternal = null;
                    }
                    else
                    {
                        ProcessorInternal = ProcessorCatalog.Instance.CreateProcessor(m_ProcessorTypeName);
                    }
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public ProcessorsNode Owner
        {
            get => m_Owner;
            set
            {
                SetField(ref m_Owner, value);
                OnPropertyChanged(nameof(First));
                OnPropertyChanged(nameof(Last));
                OnPropertyChanged(nameof(Previous));
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
                OnPropertyChanged(nameof(Previous));
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
            get => m_Owner != null && m_Owner.ProcessorNodes.Count - 1 == Index;
        }

        [JsonIgnore]
        public ProcessorNode Previous
        {
            get
            {
                if (Owner == null || Index >= Owner.ProcessorNodes.Count || Index <= 0)
                {
                    return null;
                }
                return Owner.ProcessorNodes[Index - 1];
            }
        }

        private IProcessor ProcessorInternal
        {
            get => m_ProcessorInternal;
            set
            {
                if (m_ProcessorInternal != value)
                {
                    IProcessor old = m_ProcessorInternal;
                    m_ProcessorInternal = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Parameters));
                    OnPropertyChanged(nameof(ChainFromPrevious));
                    OnPropertyChanged(nameof(ProducesFiles));
                }
            }
        }

        public bool ProducesFiles
        {
            get
            {
                if (m_ProcessorInternal == null)
                {
                    return false;
                }
                return m_ProcessorInternal.ProducesFiles;
            }
        }

        public bool ChainFromPrevious
        {
            get
            {
                if (m_ProcessorInternal == null)
                {
                    return false;
                }
                return m_ProcessorInternal.ChainFromPrevious;
            }
            set
            {
                if (m_ProcessorInternal != null)
                {
                    m_ProcessorInternal.ChainFromPrevious = value;
                }
            }
        }

        public object Parameters
        {
            get
            {
                return m_ProcessorInternal?.Parameters;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IProcessor GetProcessor()
        {
            return ProcessorInternal;
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
