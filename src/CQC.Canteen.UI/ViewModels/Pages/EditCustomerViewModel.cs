using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.Domain.Enums;
using CQC.Canteen.UI.Commands;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages;

public class EditCustomerViewModel : BaseViewModel, IDataErrorInfo
{
    private readonly ICustomerService _customerService;

    private string _name = string.Empty;
    private bool _isMilitary;
    private MilitaryRank? _rank;
    private bool _isActive;
    private bool _isSaving;
    private bool _hasUnsavedChanges;

    public int Id { get; private set; }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                _hasUnsavedChanges = true;
        }
    }

    public bool IsMilitary
    {
        get => _isMilitary;
        set
        {
            if (SetProperty(ref _isMilitary, value))
            {
                _hasUnsavedChanges = true;
                if (!_isMilitary)
                    Rank = null;
                else if (Rank == null)
                    Rank = MilitaryRank.ملازم;
            }
        }
    }

    public MilitaryRank? Rank
    {
        get => _rank;
        set
        {
            if (SetProperty(ref _rank, value))
                _hasUnsavedChanges = true;
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (SetProperty(ref _isActive, value))
                _hasUnsavedChanges = true;
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }

    public string? LastUpdatedInfo { get; set; }

    public CustomerDto? UpdatedCustomer { get; private set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public EditCustomerViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        SaveCommand = new RelayCommand<object>(async (p) => await ExecuteSaveAsync(p));
        CancelCommand = new RelayCommand<object>((p) => ((Window)p).Close());
    }

    public async Task LoadCustomerDataAsync(int customerId)
    {
        var result = await _customerService.GetCustomerDetailsByIdAsync(customerId, default);
        if (result.IsFailed)
        {
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var c = result.Value;
        Id = c.Id;
        Name = c.Name;
        IsMilitary = c.IsMilitary;
        Rank = c.Rank;
        IsActive = c.IsActive;
        LastUpdatedInfo = $"آخر تعديل بتاريخ {DateTime.Now:yyyy/MM/dd HH:mm}";
        _hasUnsavedChanges = false;
    }

    private async Task ExecuteSaveAsync(object parameter)
    {
        if (!string.IsNullOrWhiteSpace(Error))
        {
            MessageBox.Show("يرجى تصحيح الحقول قبل الحفظ.", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsSaving = true;

        var dto = new CustomerDetailsDto
        {
            Id = Id,
            Name = Name,
            IsMilitary = IsMilitary,
            Rank = IsMilitary ? Rank : null,
            IsActive = IsActive
        };

        var result = await _customerService.UpdateCustomerAsync(dto, default);
        IsSaving = false;

        if (result.IsSuccess)
        {
            UpdatedCustomer = result.Value;
            MessageBox.Show("✅ تم تحديث بيانات العميل بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            ((Window)parameter).DialogResult = true;
            ((Window)parameter).Close();
        }
        else
        {
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 🔒 تنبيه قبل الإغلاق لو في تغييرات
    public bool ConfirmClose(Window window)
    {
        if (_hasUnsavedChanges)
        {
            var result = MessageBox.Show("لم يتم حفظ التغييرات، هل تريد الخروج؟", "تحذير",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return false;
        }
        return true;
    }

    #region IDataErrorInfo
    public string Error => string.Empty;
    public string this[string columnName]
    {
        get
        {
            return columnName switch
            {
                nameof(Name) when string.IsNullOrWhiteSpace(Name) => "يجب إدخال اسم العميل.",
                nameof(Rank) when IsMilitary && Rank == null => "يجب اختيار الرتبة العسكرية.",
                _ => string.Empty
            };
        }
    }
    #endregion
}
