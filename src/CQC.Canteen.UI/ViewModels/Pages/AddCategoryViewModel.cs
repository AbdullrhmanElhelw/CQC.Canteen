using CQC.Canteen.BusinessLogic.DTOs.Categories;
using CQC.Canteen.BusinessLogic.Services.Categories;
using CQC.Canteen.UI.Commands;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages;

public class AddCategoryViewModel : BaseViewModel
{
    private readonly ICategoryService _categoryService;

    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private bool _isSaving;
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public CategoryDto NewCategory { get; private set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action RequestClose;

    public AddCategoryViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        SaveCommand = new RelayCommand<object>(async (p) => await SaveAsync(), (p) => !IsSaving);
        CancelCommand = new RelayCommand<object>((p) => RequestClose?.Invoke());
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

        var createDto = new CreateCategoryDto
        {
            Name = Name.Trim()
        };

        var result = await _categoryService.AddNewCategoryAsync(createDto, default);

        if (result.IsSuccess)
        {
            NewCategory = result.Value;
            MessageBox.Show("تم إضافة الفئة بنجاح", "نجاح",
                          MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
        else
        {
            MessageBox.Show("فشل في إضافة الفئة:\n" + string.Join("\n", result.Errors),
                          "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        IsSaving = false;
    }
}