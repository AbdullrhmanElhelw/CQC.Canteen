using CQC.Canteen.UI.ViewModels.Pages;
using System.Windows;

namespace CQC.Canteen.UI.Views.Pages;

public partial class EditCustomerView : Window
{
    public EditCustomerView(EditCustomerViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
