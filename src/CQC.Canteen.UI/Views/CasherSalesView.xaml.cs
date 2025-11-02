using System.Windows;
using System.Windows.Controls;

namespace CQC.Canteen.UI.Views;

public partial class CasherSalesView : UserControl
{
    public CasherSalesView()
    {
        InitializeComponent();
    }

    private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
    {
        var tb = (TextBox)sender;
        if (tb.Text == (string)tb.Tag)
        {
            tb.Text = "";
            tb.Foreground = System.Windows.Media.Brushes.Black;
        }
    }

    private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
    {
        var tb = (TextBox)sender;
        if (string.IsNullOrWhiteSpace(tb.Text))
        {
            tb.Text = (string)tb.Tag;
            tb.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136));
        }
    }
}
