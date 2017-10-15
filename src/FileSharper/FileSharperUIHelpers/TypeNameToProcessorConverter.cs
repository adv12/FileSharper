// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using FileSharperCore;

namespace FileSharperUIHelpers
{
    public class TypeNameToProcessorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            return ProcessorCatalog.Instance.Processors.Where(x => x.GetType().FullName == (string)value).FirstOrDefault();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.GetType().FullName;
        }
    }
}
