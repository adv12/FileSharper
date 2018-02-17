// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using FileSharperCore;

namespace FileSharperUIHelpers
{
    public class SearchViewModelToDynamicGridViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var searchViewModel = value as SearchViewModel;
            if (searchViewModel != null)
            {
                var gridView = new GridView();
                string[] columnHeaders = searchViewModel.ColumnHeaders;
                int i = 0;
                foreach (string bindingHeaderName in searchViewModel.BindingColumnHeaders)
                {
                    var binding = new Binding(bindingHeaderName);
                    gridView.Columns.Add(new GridViewColumn { Header = columnHeaders[i++],
                        DisplayMemberBinding = binding });
                }
                return gridView;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
