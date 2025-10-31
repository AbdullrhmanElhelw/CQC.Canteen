using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CQC.Canteen.UI.Converters;

/// <summary>
/// بيحول القيمة البوليانية (true/false) إلى (Visible/Collapsed)
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibilityValue)
        {
            return visibilityValue == Visibility.Visible;
        }
        return false;
    }
}
