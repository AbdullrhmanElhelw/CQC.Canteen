using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CQC.Canteen.UI.ViewModels
{
    /// <summary>
    /// الكلاس الأب لكل ViewModels
    /// بيوفر التنفيذ الأساسي لـ INotifyPropertyChanged
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// الميثود المسئولة عن إبلاغ الواجهة بالتغيير
        /// </summary>
        /// <param name="propertyName">اسم الخاصية اللي اتغيرت (بييجي أوتوماتيك)</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ميثود مساعدة لتحديث الخاصية وإبلاغ الواجهة
        /// </summary>
        /// <typeparam name="T">نوع المتغير</typeparam>
        /// <param name="backingStore">المتغير اللي بيخزن القيمة</param>
        /// <param name="value">القيمة الجديدة</param>
        /// <param name="propertyName">اسم الخاصية (أوتوماتيك)</param>
        /// <returns>True لو القيمة اتغيرت, False لو هي نفس القيمة</returns>
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
        {
            // لو القيمة الجديدة هي هي القديمة، متعملش حاجة
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            // لو القيمة اتغيرت، حدثها وبلغ الواجهة
            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

