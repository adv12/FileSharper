// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows.Controls;
using System.Windows.Data;
using FileSharperCore;

namespace FileSharperUI
{
    /// <summary>
    /// Interaction logic for OutputNodeControl.xaml
    /// </summary>
    public partial class OutputNodeControl : UserControl
    {
        public OutputNodeControl()
        {
            InitializeComponent();
            ListCollectionView lcv = new ListCollectionView(OutputCatalog.Instance.Outputs);
            lcv.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            this.comboBox.ItemsSource = lcv;
        }
    }
}
