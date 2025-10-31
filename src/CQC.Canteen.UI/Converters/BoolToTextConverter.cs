using System.Globalization;
using System.Windows.Data;

namespace CQC.Canteen.UI.Converters;

public class BoolToTextConverter : IValueConverter
{
    public string TrueText { get; set; } = "نشط";
    public string FalseText { get; set; } = "غير نشط";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueText : FalseText;
        }
        return FalseText;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}