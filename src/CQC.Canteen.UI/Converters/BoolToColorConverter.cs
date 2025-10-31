using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CQC.Canteen.UI.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ?
                new SolidColorBrush(Color.FromRgb(39, 174, 96)) : // أخضر للنشط
                new SolidColorBrush(Color.FromRgb(155, 155, 155)); // رمادي لغير النشط
        }
        return new SolidColorBrush(Color.FromRgb(155, 155, 155));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}