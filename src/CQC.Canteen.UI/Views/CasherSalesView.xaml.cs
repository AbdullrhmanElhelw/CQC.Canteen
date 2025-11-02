using CQC.Canteen.BusinessLogic.DTOs.Products;
using CQC.Canteen.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CQC.Canteen.UI.Views
{
    public partial class CasherSalesView : UserControl
    {
        private decimal _lastTotal = 0;

        public CasherSalesView()
        {
            InitializeComponent();
            DataContextChanged += CasherSalesView_DataContextChanged;
        }

        private void CasherSalesView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is CasherSalesViewModel vm)
                vm.TotalAmountChanged += Vm_TotalAmountChanged;
        }

        private void Vm_TotalAmountChanged(object? sender, decimal newTotal)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string color = newTotal > _lastTotal ? "#28A745" : "#DC3545"; // أخضر أو أحمر حسب الزيادة أو النقص
                _lastTotal = newTotal;

                var anim = new ColorAnimation
                {
                    To = (Color)ColorConverter.ConvertFromString(color),
                    Duration = TimeSpan.FromSeconds(0.4),
                    AutoReverse = true,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };

                var brush = new SolidColorBrush(Colors.Black);
                TotalText.Foreground = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            });
        }

        private void Product_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProductDto product)
            {
                if (DataContext is CasherSalesViewModel vm)
                    vm.AddToCartCommand.Execute(product);
            }
        }
    }
}
