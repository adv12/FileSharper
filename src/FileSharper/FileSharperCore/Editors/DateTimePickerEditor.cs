// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System.Windows;
using System.Windows.Data;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace FileSharperCore.Editors
{
    public class DateTimePickerEditor : DateTimePicker, ITypeEditor
    {
        public DateTimePickerEditor()
        {
            Format = DateTimeFormat.Custom;
            FormatString = "yyyy/MM/dd hh:mm:ss tt";

            TimePickerVisibility = System.Windows.Visibility.Visible;
            ShowButtonSpinner = false;
            AutoCloseCalendar = true;
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }
    }
}
