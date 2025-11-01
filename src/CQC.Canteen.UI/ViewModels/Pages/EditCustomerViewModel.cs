using CQC.Canteen.BusinessLogic.DTOs.Customers;
using CQC.Canteen.BusinessLogic.Services.Customers;
using CQC.Canteen.Domain.Enums;
using CQC.Canteen.UI.Commands;
using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI.ViewModels.Pages;

public class EditCustomerViewModel : BaseViewModel
{
    private readonly ICustomerService _customerService;

    private string _name;
    private bool _isMilitary;
    private MilitaryRank? _rank;
    private bool _isActive;

    public int Id { get; private set; }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsMilitary
    {
        get => _isMilitary;
        set
        {
            if (SetProperty(ref _isMilitary, value))
            {
                // ✅ لما النوع يتغير لـ "مدني" نحذف الرتبة
                if (!_isMilitary)
                    Rank = null;
            }
        }
    }

    public MilitaryRank? Rank
    {
        get => _rank;
        set => SetProperty(ref _rank, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

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
    }

    private async Task ExecuteSaveAsync(object parameter)
    {
        var dto = new CustomerDetailsDto
        {
            Id = Id,
            Name = Name,
            IsMilitary = IsMilitary,
            Rank = IsMilitary ? Rank : null,
            IsActive = IsActive
        };

        var result = await _customerService.UpdateCustomerAsync(dto, default);
        if (result.IsSuccess)
        {
            UpdatedCustomer = result.Value;
            MessageBox.Show("✅ تم تحديث بيانات العميل", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
            ((Window)parameter).DialogResult = true;
            ((Window)parameter).Close();
        }
        else
        {
            MessageBox.Show(string.Join("\n", result.Errors), "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
