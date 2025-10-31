using CQC.Canteen.UI.ViewModels.Pages;
using System.Windows;

namespace CQC.Canteen.UI.Views.Pages;

public partial class AddProductView : Window
{
    // هنستقبل الـ ViewModel عن طريق الـ Constructor
    public AddProductView(AddProductViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // هنا بنربط حدث "اطلب الإغلاق" (اللي في الـ VM)
        // بالـ "DialogResult" بتاع الشاشة (اللي هي الـ View)
        viewModel.RequestClose += (dialogResult) =>
        {
            try
            {
                this.DialogResult = dialogResult;
            }
            catch (InvalidOperationException)
            {
                // (بيحصل لو الشاشة اتفتحت بـ Show() بدل ShowDialog())
                this.Close();
            }
        };
    }
}
