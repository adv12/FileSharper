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
    public class OutputsNode
    {

        public ObservableCollection<OutputNode> OutputNodes
        {
            get;
        } = new ObservableCollection<OutputNode>();

        public OutputsNode()
        {
            OutputNodes.CollectionChanged += OutputNodes_CollectionChanged;
        }

        private void OutputNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (OutputNode newNode in e.NewItems)
                {
                    newNode.Owner = this;
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (e.OldItems != null)
                {
                    foreach (OutputNode oldNode in e.OldItems)
                    {
                        oldNode.Owner = null;
                    }
                }
            }
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                OutputNodes[i].Index = i;
            }
        }

        public IOutput[] GetOutputs()
        {
            return OutputNodes.Select(on => on.GetOutput()).Where(o => o != null).ToArray();
        }

        [JsonIgnore]
        public ICommand RemoveCommand { get { return new OutputNodeRemover(this); } }

        [JsonIgnore]
        public ICommand AddCommand { get { return new OutputNodeAdder(this); } }

        public class OutputNodeRemover : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public OutputsNode Node
            {
                get; set;
            }

            public OutputNodeRemover(OutputsNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Node.OutputNodes.Remove((OutputNode)parameter);
            }
        }

        public class OutputNodeAdder : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public OutputsNode Node
            {
                get; set;
            }

            public OutputNodeAdder(OutputsNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Node.OutputNodes.Add(new OutputNode());
            }
        }
    }
}
