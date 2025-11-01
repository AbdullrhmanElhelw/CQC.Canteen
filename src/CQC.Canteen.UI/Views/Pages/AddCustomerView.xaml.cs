using CQC.Canteen.UI.ViewModels.Pages;
using System.Windows;

namespace CQC.Canteen.UI.Views.Pages
{
    public partial class AddCustomerView : Window
    {
        public AddCustomerView(AddCustomerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
