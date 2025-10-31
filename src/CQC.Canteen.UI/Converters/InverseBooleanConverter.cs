using System.Globalization;
using System.Windows.Data;

namespace CQC.Canteen.UI.Converters;

/// <summary>
/// بيعكس القيمة البوليانية (true تبقى false والعكس)
/// (بنستخدمها عشان نوقف الزرار وهو بيحمل)
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}
