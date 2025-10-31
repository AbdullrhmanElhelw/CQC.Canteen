using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CQC.Canteen.UI.Converters;

public class StockToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int stock)
        {
            if (stock == 0) return new SolidColorBrush(Color.FromRgb(235, 87, 87)); // أحمر
            if (stock <= 10) return new SolidColorBrush(Color.FromRgb(242, 153, 74)); // برتقالي
            return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // أخضر
        }
        return new SolidColorBrush(Color.FromRgb(155, 155, 155)); // رمادي
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}