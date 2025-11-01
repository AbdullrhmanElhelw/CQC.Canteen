using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.UI.Commands;
using CQC.Canteen.UI.Views.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class CategoryManagementViewModel : BaseViewModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IServiceProvider _serviceProvider;

        public ObservableCollection<CategoryDto> Categories { get; } = new ObservableCollection<CategoryDto>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private CategoryDto _selectedCategory;
        public CategoryDto SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ICommand LoadCategoriesCommand { get; }
        public ICommand ShowAddCategoryDialogCommand { get; }
        public ICommand ShowEditCategoryDialogCommand { get; }

        public CategoryManagementViewModel(ICategoryService categoryService, IServiceProvider serviceProvider)
        {
            _categoryService = categoryService;
            _serviceProvider = serviceProvider;

            LoadCategoriesCommand = new RelayCommand<object>(async (p) => await LoadCategoriesAsync());
            ShowAddCategoryDialogCommand = new RelayCommand<object>((p) => ExecuteShowAddCategoryDialog());
            ShowEditCategoryDialogCommand = new RelayCommand<object>((p) => ExecuteShowEditCategoryDialog(), (p) => SelectedCategory != null);

            LoadCategoriesAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            IsLoading = true;
            Categories.Clear();

            var result = await _categoryService.GetAllCategoriesAsync(default);

            if (result.IsSuccess)
            {
                foreach (var category in result.Value)
                {
                    Categories.Add(category);
                }
            }
            else
            {
                MessageBox.Show("فشل في تحميل الفئات: " + string.Join("\n", result.Errors),
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }

        private void ExecuteShowAddCategoryDialog()
        {
            var viewModel = _serviceProvider.GetRequiredService<AddCategoryViewModel>();
            var view = new AddCategoryView(viewModel);
            view.Owner = Application.Current.MainWindow;

            var dialogResult = view.ShowDialog();

            if (dialogResult == true && viewModel.NewCategory != null)
            {
                Categories.Add(viewModel.NewCategory);
            }
        }

        private async void ExecuteShowEditCategoryDialog()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("يرجى اختيار فئة للتعديل", "تنبيه",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var viewModel = _serviceProvider.GetRequiredService<EditCategoryViewModel>();
            var view = new EditCategoryView(viewModel)
            {
                Owner = Application.Current.MainWindow
            };

            // تحميل بيانات الفئة قبل عرض النافذة
            await viewModel.LoadCategoryDataAsync(SelectedCategory.Id);

            var dialogResult = view.ShowDialog();

            if (dialogResult == true && viewModel.UpdatedCategory != null)
            {
                // تحديث العنصر في القائمة
                var existingCategory = Categories.FirstOrDefault(c => c.Id == viewModel.UpdatedCategory.Id);
                if (existingCategory != null)
                {
                    var index = Categories.IndexOf(existingCategory);
                    Categories[index] = viewModel.UpdatedCategory;
                }
            }
        }
    }
}