using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;
#endif

namespace ZapApp.AppPages
{
    public partial class ConfigPage : ContentPage
    {
        public ConfigPage()
        {
            InitializeComponent();
        }

        private async void OnSelecionarPastaClicked(object sender, EventArgs e)
        {
#if WINDOWS
            var hwnd = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                LabelCaminho.Text = folder.Path; // Atualiza a label
            }
#else
    await DisplayAlert("Não suportado", "Seleção de pasta só está implementada para Windows.", "OK");
#endif
        }

        private async void OnReturnClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
