using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CQC.Canteen.UI.Converters;

public class BooleanToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = value is bool b && b;
        return isActive
            ? new SolidColorBrush(Color.FromRgb(220, 255, 220)) // أخضر فاتح
            : new SolidColorBrush(Color.FromRgb(255, 230, 230)); // أحمر فاتح
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
