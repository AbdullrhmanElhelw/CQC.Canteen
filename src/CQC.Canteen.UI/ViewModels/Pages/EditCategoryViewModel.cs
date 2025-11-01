using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.UI.Commands;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages
{
    public class EditCategoryViewModel : BaseViewModel
    {
        private readonly ICategoryService _categoryService;

        private int _id;
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public CategoryDto UpdatedCategory { get; private set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action RequestClose;

        public EditCategoryViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;

            SaveCommand = new RelayCommand<object>(async (p) => await SaveAsync(), (p) => !IsSaving && !IsLoading);
            CancelCommand = new RelayCommand<object>((p) => RequestClose?.Invoke());
        }

        public async Task LoadCategoryDataAsync(int categoryId)
        {
            IsLoading = true;

            var result = await _categoryService.GetCategoryDetailsByIdAsync(categoryId, default);

            if (result.IsSuccess)
            {
                var category = result.Value;
                Id = category.Id;
                Name = category.Name;
            }
            else
            {
                MessageBox.Show("فشل في تحميل بيانات الفئة:\n" + string.Join("\n", result.Errors),
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }

        private async Task SaveAsync()
        {
            // التحقق من أن الاسم ليس فارغاً
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("يرجى إدخال اسم الفئة", "تنبيه",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSaving = true;

            var updateDto = new CategoryDetailsDto
            {
                Id = Id,
                Name = Name.Trim()
            };

            var result = await _categoryService.UpdateCategoryAsync(updateDto, default);

            if (result.IsSuccess)
            {
                UpdatedCategory = result.Value;
                MessageBox.Show("تم تحديث الفئة بنجاح", "نجاح",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                RequestClose?.Invoke();
            }
            else
            {
                MessageBox.Show("فشل في تحديث الفئة:\n" + string.Join("\n", result.Errors),
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsSaving = false;
        }
    }
}