// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FileSharperCore.Conditions;
using Newtonsoft.Json;

namespace FileSharperCore
{
    public class ConditionNode: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_ConditionTypeName;
        private bool m_Not;
        private ConditionNode m_Owner;
        private int m_Index;
        private ICondition m_ConditionInternal;
        private bool m_Loaded;

        [JsonProperty(Order = int.MinValue)]
        public string ConditionTypeName
        {
            get => m_ConditionTypeName;
            set {
                if (m_ConditionTypeName != value)
                {
                    ConditionNode[] oldChildren = ChildNodes.ToArray();
                    ICondition oldCondition = m_ConditionInternal;
                    ChildNodes.Clear();
                    m_ConditionTypeName = value;
                    if (value == null)
                    {
                        ConditionInternal = null;
                    }
                    else
                    {
                        ConditionInternal = ConditionCatalog.Instance.CreateCondition(m_ConditionTypeName);
                        if (ConditionInternal is CompoundCondition)
                        {
                            if (oldChildren.Length > 0)
                            {
                                foreach (ConditionNode child in oldChildren)
                                {
                                    ChildNodes.Add(child);
                                }
                            }
                            else if (Loaded)
                            {
                                ConditionNode starterNode = new ConditionNode();
                                if (oldCondition != null)
                                {
                                    starterNode.ConditionTypeName = oldCondition.GetType().FullName;
                                    starterNode.ConditionInternal = oldCondition;
                                }
                                ChildNodes.Add(starterNode);
                            }
                        }
                    }
                    OnPropertyChanged();
                }
            }
        }

        public bool Not
        {
            get => m_Not;
            set => SetField(ref m_Not, value);
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
                    foreach (ConditionNode child in ChildNodes)
                    {
                        child.Loaded = Loaded;
                    }
                }
            }
        }

        [JsonIgnore]
        public ConditionNode Owner
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
        public bool First => Index == 0;

        [JsonIgnore]
        public bool Last => m_Owner != null && m_Owner.ChildNodes.Count - 1 == Index;

        private ICondition ConditionInternal
        {
            get => m_ConditionInternal;
            set
            {
                if (m_ConditionInternal != value)
                {
                    ICondition old = m_ConditionInternal;
                    m_ConditionInternal = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Parameters));
                }
            }
        }

        public object Parameters => m_ConditionInternal?.Parameters;

        public ObservableCollection<ConditionNode> ChildNodes
        {
            get;
        } = new ObservableCollection<ConditionNode>();

        public ConditionNode()
        {
            ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;
        }

        private void ChildNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ConditionNode newNode in e.NewItems)
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
                    foreach (ConditionNode oldNode in e.OldItems)
                    {
                        oldNode.Owner = null;
                    }
                }
            }
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                ChildNodes[i].Index = i;
            }
        }

        public ICondition BuildCondition()
        {
            ICondition condition = ConditionInternal;
            if (condition is CompoundCondition)
            {
                CompoundCondition cc = (CompoundCondition)condition;
                cc.Conditions.Clear();
                foreach (ConditionNode child in ChildNodes)
                {
                    ICondition childCondition = child.BuildCondition();
                    if (childCondition != null)
                    {
                        cc.Conditions.Add(childCondition);
                    }
                }
            }
            if (condition != null && Not)
            {
                condition = new NotCondition(condition);
            }
            if (condition == null)
            {
                condition = new MatchEverythingCondition();
            }
            return condition;
        }

        [JsonIgnore]
        public ICommand RemoveCommand { get { return new ChildNodeRemover(this); } }

        [JsonIgnore]
        public ICommand AddCommand { get { return new ChildNodeAdder(this); } }

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

        public class ChildNodeRemover : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public ConditionNode Node
            {
                get; set;
            }

            public ChildNodeRemover(ConditionNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                Node.ChildNodes.Remove((ConditionNode)parameter);
            }
        }

        public class ChildNodeAdder : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public ConditionNode Node
            {
                get; set;
            }

            public ChildNodeAdder(ConditionNode node)
            {
                Node = node;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                ConditionNode newNode = new ConditionNode();
                newNode.Loaded = Node.Loaded;
                Node.ChildNodes.Add(newNode);
            }
        }
    }
}
