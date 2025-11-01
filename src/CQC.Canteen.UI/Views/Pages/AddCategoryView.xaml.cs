using CQC.Canteen.UI.ViewModels.Pages;
using System.Windows;

namespace CQC.Canteen.UI.Views.Pages;

public partial class AddCategoryView : Window
{
    public AddCategoryView(AddCategoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // ربط حدث الإغلاق
        viewModel.RequestClose += () =>
        {
            DialogResult = true;
            Close();
        };
    }
}