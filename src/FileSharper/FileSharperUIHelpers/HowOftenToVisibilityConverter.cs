// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FileSharperCore;

namespace FileSharperUIHelpers
{
    public class HowOftenToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            HowOften howOften = (value == null ? HowOften.Never : (HowOften)value);
            bool reverse = (parameter == null ? false : (bool)parameter);
            bool visible = reverse ? howOften == HowOften.Never : howOften != HowOften.Never;
            if (visible)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
