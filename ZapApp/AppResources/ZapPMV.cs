using ClosedXML.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions; // Necessário para Regex
using System.Threading;
using System.Threading.Tasks;

namespace ZapApp.AppResources
{

    public class ZapPMV
    {        
        public string path_excel = "D:\\pdf\\Lista\\agendamento.xlsx";
        public string path_txt = "D:\\pdf\\Lista\\erro_" + DateTime.Now.ToString("yyyy-MM-dd") + "_.txt";
        public string path_pdf = "D:\\pdf\\pdf";
        public bool control_pc = false;
        public bool control_tipo = false;
        public async Task OpenExcelAsync(IWebDriver driver)
        {
            var banco = new BancoDados();
            string conteudoDoErro = "Log Zap dia: " + DateTime.Now.ToString("yyyy-MM-dd") + "\n\n";
            await File.WriteAllTextAsync(path_txt, conteudoDoErro);
            using (var workbook = new XLWorkbook(path_excel))
            {
                var worksheet = workbook.Worksheet(1); // primeira aba
                var lastRow = worksheet.LastRowUsed().RowNumber();

                for (int row = 1; row <= lastRow; row++) // pula o cabeçalho
                {
                    var mensagens = new List<string>();
                    var telefones = new List<string>();
                    var situacao = worksheet.Cell(row, 9).GetString();       // Coluna I
                    var dadosPessoa = worksheet.Cell(row, 4).GetString();    // Coluna D
                    var infoProcedimento = worksheet.Cell(row, 6).GetString(); // Coluna F

                    string cns = "", nome = "", procedimento = "", local = "", data = "", tipo = "";
                    DateTime apenasData = DateTime.MinValue;

                    // Nome e telefone
                    var linhas = dadosPessoa.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (linhas.Length > 0)
                        nome = linhas[0].Trim();


                    foreach (var linha in linhas)
                    {
                        if (linha.Contains("Tel:"))
                        {
                            var extraido = linha.Replace("Tel:", "").Trim();
                            var encontrados = TelefoneUtils.ExtrairTelefonesCelulares(extraido);
                            telefones.AddRange(encontrados);
                        }

                        if (linha.Contains("CNS:"))
                        {
                            cns = linha.Replace("CNS:", "").Trim();
                        }
                    }

                    // Procedimento, local, data
                    var partes = infoProcedimento.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length > 0)
                        procedimento = partes[0].Trim();

                    foreach (var linha in partes)
                    {
                        if (linha.StartsWith("Agendado p/") || linha.StartsWith("Aprovado p/"))
                        {
                            var split = linha.Split("p/");
                            if (split.Length > 1)
                                local = split[1].Trim();
                        }

                        if (linha.StartsWith("em "))
                        {
                            data = linha.Replace("em", "").Trim();

                            if (DateTime.TryParseExact(data, "dd/MM/yyyy - HH:mm",
                                System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime dataComHora))
                            {
                                // Cria novo DateTime somente com dia, mês e ano (hora zerada)
                                apenasData = new DateTime(dataComHora.Year, dataComHora.Month, dataComHora.Day);
                                Console.WriteLine(apenasData.ToString("dd/MM/yyyy"));  // Ex: 23/06/2025
                            }
                        }

                    }

                    // Monta a mensagem de acordo com a situação
                    string mensagem = "";
                    if (situacao.Contains("Agendado") || situacao.Contains("Aprovado"))
                    {
                        tipo = "PMV";
                        control_tipo = true;
                        mensagem = $"Senhor(a) {nome}, a Unidade de Saúde de Itararé informa.\n" +
                            $"Sua especialidade {procedimento} foi agendado para o dia {data}.\n" +
                            $"Segue o agendamento, não é necessário comparecer na unidade de saúde.\n" +
                            $"Caso necessite cancelar ou reagendar, favor procurar a sala de especialidades.\n" +
                            $"Esta é uma mensagem automática, favor não responder. Grato";
                    }
                    if (situacao.Contains("Agendado no Estado"))
                    {
                        tipo = "Estado";
                        mensagem = $"Senhor(a) {nome}, a Unidade de Saúde de Itararé informa.\n" +
                            $"Sua especialidade {procedimento} foi agendado.\n" +
                            $"Favor retirar o comprovante em até 1 dia útil na unidade de saúde de 8h até 16:30h.\n" +
                            $"Esta é uma mensagem automática, favor não responder. Grato";
                    }
                    for (int i = 0; i < telefones.Count; i++)
                    {
                        string log;
                        bool sucesso = await SendMessage(driver, mensagem, telefones[i], nome);
                        if (sucesso)
                        {
                            log = $"Mensagem enviada com sucesso para {nome} ({telefones[i]}).\n";                      
                            control_pc = true;
                        }
                        else
                        {
                            log = $"ERRO ao enviar mensagem para {nome} ({telefones[i]}). Verifique o número ou a conexão.\n";                            
                            //continue;
                        }
                        await File.AppendAllTextAsync(path_txt, log);
                        banco.SalvarRegistro(cns: cns,
                                            nome: nome,
                                            procedimento: procedimento,
                                            local: tipo,
                                            data_agenda: apenasData,
                                            telefone: telefones[i],
                                            enviado: sucesso,
                                            data_zap:DateTime.Now);
                        
                    }
                    if (!control_pc)
                    {
                        string log = $"Sem números válidos para {nome}.\n\n";
                        await File.AppendAllTextAsync(path_txt, log);
                    }
                }
            }
            driver.Close();
        }

        public async Task<bool> SendMessage(IWebDriver driver, string msg, string tel, string nome)
        {
            bool validar_numero()
            {
                try
                {
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    string encodedMsg = Uri.EscapeDataString(msg);

                    string url = $"https://web.whatsapp.com/send?phone={tel}&text={encodedMsg}";

                    Debug.WriteLine($"Navegando para o chat com {tel} para enviar a mensagem...");
                    driver.Navigate().GoToUrl(url);
                    IWebElement sendButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Enviar']")));
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERRO ao navegar para o chat com {tel}: {ex.Message}");
                    return false;
                }
            }
            if (!validar_numero())
            {
                Debug.WriteLine($"Número {tel} inválido ou erro ao navegar para o chat.");
                return false;
            }
            else
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                Debug.WriteLine("Aguardando o botão de enviar aparecer e ser clicável...");
                IWebElement sendButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Enviar']")));

                Debug.WriteLine("Botão encontrado. Enviando a mensagem...");
                sendButton.Click();

                await Task.Delay(2000);
                Debug.WriteLine($"Mensagem enviada com sucesso para {nome} ({tel}).");

                Debug.WriteLine("Iniciando processo de envio de PDFs...");
                await pedido_pdf(driver, tel, nome);
                await regulacao_pdf(driver, tel, nome);
                await mmg_pdf(driver, tel, nome);
                return true;
            }
        }

        public async Task pedido_pdf(IWebDriver driver, string tel, string nome)
        {
            string nomeDoArquivo = nome + "_pedido.pdf";
            string caminhoCompletoDoArquivo = Path.Combine(path_pdf, nomeDoArquivo);

            try
            {
                if (!Directory.Exists(path_pdf))
                {
                    Directory.CreateDirectory(path_pdf);
                }

                if (File.Exists(caminhoCompletoDoArquivo))
                {
                    await EnviarArquivoPeloWhatsApp(driver, tel, caminhoCompletoDoArquivo);
                }
            }
            catch
            {
                
            }
        }

        public async Task regulacao_pdf(IWebDriver driver, string tel, string nome)
        {
            string nomeDoArquivo = nome + ".pdf";
            string caminhoCompletoDoArquivo = Path.Combine(path_pdf, nomeDoArquivo);

            try
            {
                if (!Directory.Exists(path_pdf))
                {
                    Directory.CreateDirectory(path_pdf);
                }

                if (File.Exists(caminhoCompletoDoArquivo))
                {
                    await EnviarArquivoPeloWhatsApp(driver, tel, caminhoCompletoDoArquivo);
                }
            }
            catch
            {

            }
        }
        public async Task mmg_pdf(IWebDriver driver, string tel, string nome)
        {
            string nomeDoArquivo = nome + "_mmg.pdf";
            string caminhoCompletoDoArquivo = Path.Combine(path_pdf, nomeDoArquivo);

            try
            {
                if (!Directory.Exists(path_pdf))
                {
                    Directory.CreateDirectory(path_pdf);
                }

                if (File.Exists(caminhoCompletoDoArquivo))
                {
                    await EnviarArquivoPeloWhatsApp(driver, tel, caminhoCompletoDoArquivo);
                }
            }
            catch
            {

            }
        }
        private async Task EnviarArquivoPeloWhatsApp(IWebDriver driver, string telefone, string caminhoDoArquivo)
        {
            try
            {
                if (!File.Exists(caminhoDoArquivo))
                {
                    Debug.WriteLine($"ERRO: Arquivo não encontrado, pulando envio para {telefone}. Caminho: {caminhoDoArquivo}");
                    return;
                }

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));

                Debug.WriteLine($"Navegando para a conversa com o número: {telefone}");
                driver.Navigate().GoToUrl($"https://web.whatsapp.com/send?phone={telefone}&text=");

                Debug.WriteLine("Aguardando a página do WhatsApp carregar...");
                IWebElement clipButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("span[data-icon='plus-rounded']")));

                Debug.WriteLine("Clicando no botão de anexo...");
                clipButton.Click();

                Debug.WriteLine("Tentando encontrar o input de arquivo de documentos diretamente...");
                var inputElements = driver.FindElements(By.CssSelector("input[type='file']"));
                var inputFile = inputElements.FirstOrDefault();
                if (inputFile != null)
                {
                    inputFile.SendKeys(caminhoDoArquivo);
                    Console.WriteLine("Arquivo anexado.");
                    await Task.Delay(3000);
                    try
                    {
                        IWebElement sendButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("span[data-icon='wds-ic-send-filled']")));
                        sendButton.Click();
                        await Task.Delay(4000);
                        Debug.WriteLine("Botão de enviar clicado com sucesso.");
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Debug.WriteLine("Erro: O botão de enviar não se tornou clicável dentro do tempo esperado.");
                    }

                }
                else
                {
                    Console.WriteLine("Input de arquivo não encontrado.");
                }

                Debug.WriteLine($"Arquivo enviado com sucesso para {telefone}. Aguardando um momento...");
                await Task.Delay(3000);
            }
            catch (WebDriverTimeoutException ex)
            {
                Debug.WriteLine($"ERRO DE TIMEOUT ao enviar para {telefone}: O elemento esperado não foi encontrado a tempo. Verifique os seletores e a conexão. Mensagem: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERRO INESPERADO ao enviar arquivo para {telefone}: {ex.Message}");
            }
        }
    }
}
