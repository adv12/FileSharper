// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Globalization;
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
                foreach (string headerName in searchViewModel.ColumnHeaders)
                {
                    var binding = new Binding(headerName);
                    gridView.Columns.Add(new GridViewColumn { Header = headerName, DisplayMemberBinding = binding });
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
