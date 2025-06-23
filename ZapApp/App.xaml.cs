using ZapApp.AppResources;
namespace ZapApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            VerificarPermissaoInicial();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
        private async void VerificarPermissaoInicial()
        {
            var permission = new Permission();
            bool permitido = await permission.VerificarPermissaoOnlineAsync();

            if (!permitido)
            {
                // Exibe página de bloqueio personalizada ou alerta
                MainPage = new ContentPage
                {
                    Content = new Label
                    {
                        Text = "O sistema está temporariamente bloqueado.",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                        FontSize = 18,
                        TextColor = Colors.Red
                    }
                };
            }
            else
            {
                // Permissão OK → carregar a tela principal
                MainPage = new AppPages.StartPage();
            }
        }
    }
}