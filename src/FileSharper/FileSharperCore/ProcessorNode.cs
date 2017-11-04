// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileSharperCore.Processors;
using Newtonsoft.Json;

namespace FileSharperCore
{
    public class ProcessorNode : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_ProcessorTypeName;
        private ProcessorsNode m_Owner;
        private ProcessorNode m_Parent;
        private int m_Index;
        private ProcessorsNode m_ChildProcessorsNode;
        private bool m_Loaded;

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
                        if (ProcessorInternal is MultiProcessor)
                        {
                            ChildProcessorsNode = new ProcessorsNode();
                            ChildProcessorsNode.Owner = this;
                            ChildProcessorsNode.Loaded = this.Loaded;
                            if (Loaded)
                            {
                                ChildProcessorsNode.AddCommand.Execute(null);
                            }
                        }
                        else
                        {
                            if (ChildProcessorsNode != null)
                            {
                                ChildProcessorsNode.Owner = null;
                            }
                            ChildProcessorsNode = null;
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool Loaded
        {
            get
            {
                return m_Loaded;
            }
            set
            {
                if (m_Loaded != value)
                {
                    m_Loaded = value;
                    if (m_ChildProcessorsNode != null)
                    {
                        m_ChildProcessorsNode.Loaded = value;
                    }
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
                UpdateInputFileSources();
            }
        }

        [JsonIgnore]
        public ProcessorNode Parent
        {
            get => m_Parent;
            set
            {
                SetField(ref m_Parent, value);
                UpdateInputFileSources();
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
                UpdateInputFileSources();
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

        [JsonIgnore]
        public ObservableCollection<InputFileSource> InputFileSources { get; } = new ObservableCollection<InputFileSource>();

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
                    OnPropertyChanged(nameof(InputFileSource));
                }
            }
        }

        public InputFileSource InputFileSource
        {
            get
            {
                if (m_ProcessorInternal == null)
                {
                    return InputFileSource.OriginalFile;
                }
                return m_ProcessorInternal.InputFileSource;
            }
            set
            {
                if (m_ProcessorInternal != null)
                {
                    m_ProcessorInternal.InputFileSource = value;
                    OnPropertyChanged();
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

        public ProcessorsNode ChildProcessorsNode
        {
            get => m_ChildProcessorsNode;
            set => SetField(ref m_ChildProcessorsNode, value);
        }

        public ProcessorNode()
        {
            InputFileSources.Add(InputFileSource.OriginalFile);
        }

        private void UpdateInputFileSources()
        {
            if (m_Parent != null)
            {
                if (!InputFileSources.Contains(InputFileSource.ParentInput))
                {
                    InputFileSources.Add(InputFileSource.ParentInput);
                }
                InputFileSources.Remove(InputFileSource.PreviousOutput);
            }
            else
            {
                if (!InputFileSources.Contains(InputFileSource.PreviousOutput))
                {
                    if (!First)
                    {
                        InputFileSources.Add(InputFileSource.PreviousOutput);
                    }
                }
                InputFileSources.Remove(InputFileSource.ParentInput);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IProcessor GetProcessor()
        {
            IProcessor processor = ProcessorInternal;
            if (processor is MultiProcessor)
            {
                MultiProcessor mp = (MultiProcessor)processor;
                mp.Processors.Clear();
                mp.Processors.AddRange(ChildProcessorsNode.GetProcessors());
            }
            return processor;
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
