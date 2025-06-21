using ZapApp.AppPages;

namespace ZapApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeRoute();
            InitializeComponent();
        }

        public static void InitializeRoute()
        {
            Routing.RegisterRoute("StartPage", typeof(StartPage));           
            Routing.RegisterRoute("ConfigPage", typeof(ConfigPage));
            Routing.RegisterRoute("SearchPage", typeof(SearchPage));
        }
    }
}
