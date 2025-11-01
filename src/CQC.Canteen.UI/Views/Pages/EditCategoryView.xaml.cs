using CQC.Canteen.UI.ViewModels.Pages;
using System.Windows;

namespace CQC.Canteen.UI.Views.Pages;

public partial class EditCategoryView : Window
{
    public EditCategoryView(EditCategoryViewModel viewModel)
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