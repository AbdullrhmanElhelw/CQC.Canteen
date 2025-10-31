using System.Windows.Input;

namespace CQC.Canteen.UI.Commands;

/// <summary>
/// النسخة "الجينيريك" من الريلاي كوماند اللي بتقبل باراميتر
/// </summary>
/// <typeparam name="T">نوع الباراميتر اللي هيتبعت (زي PasswordBox)</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        // لو الـ parameter مش من نفس النوع T، رجع false
        if (parameter != null && parameter is not T)
        {
            return false;
        }

        // لو مفيش شرط (canExecute is null)، يبقى دايماً ينفع
        // لو فيه شرط، نفذه
        return _canExecute == null || _canExecute((T)parameter);
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }
}

