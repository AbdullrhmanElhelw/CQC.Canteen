using CQC.Canteen.UI.ViewModels.Pages;
using System.Windows;

namespace CQC.Canteen.UI.Views.Pages;

public partial class EditProductView : Window
{
    public EditProductView(EditProductViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.RequestClose += (dialogResult) =>
        {
            try
            {
                this.DialogResult = dialogResult;
            }
            catch (InvalidOperationException)
            {
                this.Close();
            }
        };
    }
}