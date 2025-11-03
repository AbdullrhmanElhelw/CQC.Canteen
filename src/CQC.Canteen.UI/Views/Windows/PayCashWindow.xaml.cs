using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CQC.Canteen.UI.Views.Windows;

public partial class PayCashWindow : Window
{
    private readonly decimal _totalAmount;

    // خصائص عامة ليقرأها الـ ViewModel
    public bool ShouldPrintReceipt { get; private set; }
    public decimal AmountReceived { get; private set; }
    public decimal Change { get; private set; }

    public PayCashWindow(decimal totalAmount)
    {
        InitializeComponent();
        _totalAmount = totalAmount;
        TotalAmountText.Text = _totalAmount.ToString("N2") + " ج.م";
        UpdateChange(); // لحساب الباقي الافتراضي
    }

    private void AmountReceivedBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!this.IsLoaded)
        {
            return;
        }

        UpdateChange();
    }

    private void UpdateChange()
    {
        if (!decimal.TryParse(AmountReceivedBox.Text, out decimal receivedAmount))
        {
            receivedAmount = 0;
        }

        decimal change = receivedAmount - _totalAmount;

        // تخزين القيم ليقرأها الـ ViewModel
        this.AmountReceived = receivedAmount;
        this.Change = change;

        ChangeText.Text = change.ToString("N2") + " ج.م";

        // تغيير لون الباقي وتفعيل/تعطيل الزر
        if (change < 0)
        {
            ChangeText.Foreground = System.Windows.Media.Brushes.Red;
            ConfirmButton.IsEnabled = false;
        }
        else
        {
            ChangeText.Foreground = System.Windows.Media.Brushes.DarkGreen;
            ConfirmButton.IsEnabled = true;
        }
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        // تخزين اختيار الطباعة
        ShouldPrintReceipt = PrintReceiptCheckBox.IsChecked ?? false;

        DialogResult = true;
        this.Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        this.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // جعل التركيز على صندوق الإدخال مباشرة
        AmountReceivedBox.SelectAll();
        AmountReceivedBox.Focus();

        // جعل زر Enter يضغط "تأكيد"
        AmountReceivedBox.KeyDown += (s, ev) =>
        {
            if (ev.Key == Key.Enter && ConfirmButton.IsEnabled)
            {
                Confirm_Click(s, ev);
            }
        };
    }
}