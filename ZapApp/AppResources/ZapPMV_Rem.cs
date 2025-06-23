using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZapApp.AppResources
{
    public class ZapPMV_Rem
    {
        public async Task<int> List_DB(IWebDriver driver)
        {
            using var db = new AppDbContext();
            DateTime dataAgenda = DateTime.Now.AddDays(1);

            var resultados = await db.Registros
                .Where(r => r.Data_Agenda.Date == dataAgenda.Date && r.Local == "PMV")
                .ToListAsync();
            if (!resultados.Any())
                return 0;

            foreach (var registro in resultados)
            {
                string msg = $"Senhor(a) {registro.Nome}, a Unidade de Saúde de Itararé informa.\n" +
                             $"Conforme já avisado, {registro.Procedimento} está agendado para amanhã.\n" +
                             $"Caso tenha cancelado ou reagendado, desconsiderar a mensagem.\n" +
                             $"Esta é uma mensagem automática, favor não responder. Grato";

                string tel = registro.Telefone;
                await SendMsgZap(driver, tel, msg);
            }
            driver.Close();
            return resultados.Count;

        }
        public async Task SendMsgZap(IWebDriver driver, string tel, string msg)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                string encodedMsg = Uri.EscapeDataString(msg);

                string url = $"https://web.whatsapp.com/send?phone={tel}&text={encodedMsg}";

                Debug.WriteLine($"Navegando para o chat com {tel} para enviar a mensagem...");
                driver.Navigate().GoToUrl(url);
                IWebElement sendButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Enviar']")));
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO ao navegar para o chat com {tel}: {ex.Message}");
            }
        }
    }

}
