using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace HeatProductionOptimization.ViewModels
{
    public class IsNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
                
            if (value is string stringValue)
                return !string.IsNullOrWhiteSpace(stringValue);
                
            if (value is int intValue)
                return intValue != 0;
                
            if (value is double doubleValue)
                return doubleValue != 0;
                
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}