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
    public class FieldSourcesNode
    {

        public ObservableCollection<FieldSourceNode> FieldSourceNodes
        {
            get;
        } = new ObservableCollection<FieldSourceNode>();

        public FieldSourcesNode()
        {
            FieldSourceNodes.CollectionChanged += FieldSourceNodes_CollectionChanged;
        }

        private void FieldSourceNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FieldSourceNode newNode in e.NewItems)
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
                    foreach (FieldSourceNode oldNode in e.OldItems)
                    {
                        oldNode.Owner = null;
                    }
                }
            }
            for (int i = 0; i < FieldSourceNodes.Count; i++)
            {
                FieldSourceNodes[i].Index = i;
            }
        }

        public IFieldSource[] GetFieldSources()
        {
            return FieldSourceNodes.Select(on => on.GetFieldSource()).Where(o => o != null).ToArray();
        }

        [JsonIgnore]
        public ICommand RemoveCommand { get { return new FieldSourceNodeRemover(this); } }

        [JsonIgnore]
        public ICommand AddCommand { get { return new FieldSourceNodeAdder(this); } }

        public class FieldSourceNodeRemover : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public FieldSourcesNode Node
            {
                get; set;
            }

            public FieldSourceNodeRemover(FieldSourcesNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Node.FieldSourceNodes.Remove((FieldSourceNode)parameter);
            }
        }

        public class FieldSourceNodeAdder : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public FieldSourcesNode Node
            {
                get; set;
            }

            public FieldSourceNodeAdder(FieldSourcesNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Node.FieldSourceNodes.Add(new FieldSourceNode());
            }
        }
    }
}
