using CQC.Canteen.BusinessLogic;
using CQC.Canteen.Domain;
using CQC.Canteen.UI.ViewModels; // عشان MainViewModel و LoginViewModel
using CQC.Canteen.UI.ViewModels.Pages;
using CQC.Canteen.UI.Views;       // عشان LoginView
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CQC.Canteen.UI
{
    public static class ServicesRegistration
    {
        public static void RegisterAppServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. تسجيل طبقات الـ Backend
            services.AddDomain(configuration);
            services.AddBusniessLogic();


            services.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            // (!! السطر الجديد المهم !!)
            // الـ LoginViewModel بيتسجل كـ Transient عشان لو عملنا Logout
            services.AddTransient<LoginViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginView>();


            // 3. تسجيل الـ Views
            // (!! السطر الجديد المهم !!)
            services.AddTransient<AdminDashboardViewModel>();
            services.AddTransient<CasherSalesViewModel>();
            services.AddTransient<ProductManagementViewModel>();
            services.AddTransient<EditProductViewModel>();
            services.AddTransient<AddProductViewModel>();
            services.AddTransient<CategoryManagementViewModel>();
            services.AddTransient<AddCategoryViewModel>();
            services.AddTransient<EditCategoryViewModel>();

        }
    }
}

