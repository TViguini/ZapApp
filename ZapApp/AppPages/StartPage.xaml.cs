using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using ZapApp.AppResources;

namespace ZapApp.AppPages;

public partial class StartPage : ContentPage
{
    IWebDriver? driver;
    string userZapdatadir = Path.Combine(AppContext.BaseDirectory, "UserData");

    public StartPage()
	{
		InitializeComponent();
	}
    private void OnZapExcelClicked(object sender, EventArgs e)
    {
        var options = new EdgeOptions();
        options.AddArgument($"--user-data-dir={userZapdatadir}");
        options.AddArgument("--profile-directory=Default");

        driver = new EdgeDriver(options);
        driver.Navigate().GoToUrl("https://web.whatsapp.com/");

        while (driver.FindElements(By.Id("pane-side")).Count < 1)
        {
            Thread.Sleep(1000); // Aguarda 1 segundo
        }
        ZapPMV zapPMV = new ZapPMV();
        zapPMV.OpenExcelAsync(driver);
    }

    private async void OnConfigClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ConfigPage");
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SearchPage");
    }

    private async void OnRememberClicked(object sender, EventArgs e)
    {
        using var db = new AppDbContext();
        ZapPMV_Rem zapPMV_Rem = new ZapPMV_Rem();
        int registros = await zapPMV_Rem.List_DB_Num(db, userZapdatadir, driver);

       if (registros == 0)
       {
            await DisplayAlert("ZapPMV", $"Nenhum registro encontrado!", "OK");
       }
    }
}