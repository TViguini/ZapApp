using ZapApp.AppResources;
using ZapApp.AppPages;
namespace ZapApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell(); // Sempre inicia com Shell
            VerificarPermissaoInicial();
        }

        private async void VerificarPermissaoInicial()
        {
            var permission = new Permission();
            bool permitido = await permission.VerificarPermissaoOnlineAsync();

            if (!permitido)
            {
                await Shell.Current.GoToAsync("//BloqueioPage"); // página dentro do Shell
            }
            else
            {
                await Shell.Current.GoToAsync("//StartPage"); // página inicial dentro do Shell
            }
        }
    }


}