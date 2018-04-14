using System;
using System.Windows.Data;

namespace FileSharperUIHelpers
{
    public class SubtractNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                if (parameter is double)
                {
                    return (double)value - (double)parameter;
                }
                return value;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
