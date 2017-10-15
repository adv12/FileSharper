// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ookii.Dialogs.Wpf;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace FileSharperCore.Editors
{
    /// <summary>
    /// Interaction logic for OpenFileEditor.xaml
    /// </summary>
    public partial class FolderChooserEditor : UserControl, ITypeEditor
    {
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public FolderChooserEditor()
        {
            InitializeComponent();
        }

        // Using a DependencyProperty as the backing store for Value.This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(FolderChooserEditor), new PropertyMetadata(null));

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }

        private void chooseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog fd = new VistaFolderBrowserDialog();
            fd.SelectedPath = Value;
            if (fd.ShowDialog() == true)
            {
                Value = fd.SelectedPath;
            }
        }
    }
}
