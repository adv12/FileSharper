// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows.Controls;
using System.Windows.Data;
using FileSharperCore;

namespace FileSharperUI
{
    /// <summary>
    /// Interaction logic for ConditionNodeControl.xaml
    /// </summary>
    public partial class ConditionNodeControl : UserControl
    {

        public ConditionNodeControl()
        {
            InitializeComponent();
            ListCollectionView lcv = new ListCollectionView(ConditionCatalog.Instance.Conditions);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            this.comboBox.ItemsSource = lcv;
        }

    }
}
