// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Newtonsoft.Json;

namespace FileSharperCore
{
    public class ProcessorsNode
    {
        private bool m_Loaded;

        public ObservableCollection<ProcessorNode> ProcessorNodes
        {
            get;
        } = new ObservableCollection<ProcessorNode>();

        [JsonIgnore]
        public ProcessorNode Owner
        {
            get; set;
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
                    foreach (ProcessorNode processor in ProcessorNodes)
                    {
                        processor.Loaded = Loaded;
                    }
                }
            }
        }

        public ProcessorsNode()
        {
            ProcessorNodes.CollectionChanged += ProcessorNodes_CollectionChanged;
            if (Loaded)
            {
                AddCommand.Execute(null);
            }
        }

        private void ProcessorNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ProcessorNode newNode in e.NewItems)
                {
                    newNode.Owner = this;
                    newNode.Parent = Owner;
                    newNode.PropertyChanged += NodePropertyChanged;
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (e.OldItems != null)
                {
                    foreach (ProcessorNode oldNode in e.OldItems)
                    {
                        oldNode.Owner = null;
                        oldNode.Parent = null;
                        oldNode.PropertyChanged -= NodePropertyChanged;
                    }
                }
            }
            for (int i = 0; i < ProcessorNodes.Count; i++)
            {
                ProcessorNode node = ProcessorNodes[i];
                node.Index = i;
                if (i == 0 && node.InputFileSource == InputFileSource.PreviousOutput)
                {
                    node.InputFileSource = InputFileSource.OriginalFile;
                }
            }
        }

        private void NodePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // TODO: May not need this anymore.  For now, wait and see.
        }

        public IProcessor[] GetProcessors()
        {
            return ProcessorNodes.Select(pn => pn.GetProcessor()).Where(p => p != null).ToArray();
        }

        [JsonIgnore]
        public ICommand RemoveCommand { get { return new ProcessorNodeRemover(this); } }

        [JsonIgnore]
        public ICommand AddCommand { get { return new ProcessorNodeAdder(this); } }

        public class ProcessorNodeRemover : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public ProcessorsNode Node
            {
                get; set;
            }

            public ProcessorNodeRemover(ProcessorsNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Node.ProcessorNodes.Remove((ProcessorNode)parameter);
            }
        }

        public class ProcessorNodeAdder : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public ProcessorsNode Node
            {
                get; set;
            }

            public ProcessorNodeAdder(ProcessorsNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ProcessorNode node = new ProcessorNode();
                node.Loaded = this.Node.Loaded;
                Node.ProcessorNodes.Add(node);
            }
        }
    }
}
