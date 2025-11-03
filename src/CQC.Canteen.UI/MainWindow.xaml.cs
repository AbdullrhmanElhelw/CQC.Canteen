using System.Windows;
using System.Windows.Input;

namespace CQC.Canteen.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Handle window dragging
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        // Minimize button
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Maximize/Restore button
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        // Close button
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user is logged in and confirm
            if (DataContext is ViewModels.MainViewModel viewModel && viewModel.IsLoggedIn)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد أنك تريد إغلاق البرنامج؟",
                    "تأكيد الإغلاق",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                    return;
            }

            Application.Current.Shutdown();
        }
    }
}