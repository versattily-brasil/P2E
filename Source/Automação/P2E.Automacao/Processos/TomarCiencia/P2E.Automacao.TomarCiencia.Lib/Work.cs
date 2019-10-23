﻿using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using P2E.Automacao.Entidades;
using P2E.Automacao.Shared.Extensions;
using P2E.Automacao.Shared.Log;
using P2E.Automacao.Shared.Log.Enum;
using P2E.Automacao.TomarCiencia.Lib.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace P2E.Automacao.TomarCiencia.Lib
{
    public class Work
    {
        #region Variaveis Estáticas
        private string _urlPrincipal = @"https://online.sefaz.am.gov.br/inicioDte.asp";
        private string _urlIncricao = @"https://online.sefaz.am.gov.br/dte/sel_inscricao_pf.asp?inscricao=";
        private string _urlConsultaDI = @"https://online.sefaz.am.gov.br/sinf2004/DI/pagDIOnline.asp?numPagina=";
        private string _urlApiBase;
        private string _urlDocLacre = @"https://online.sefaz.am.gov.br/sinf2004/DI/rel_lacre.asp?idDi=";
        private List<Importacao> ListaProcessosBD;
        int _cd_bot_exec;
        int _cd_par;
        private string razaoSocial = "";
        private string CNPJ = "";
        private string inscricaoEstadual = "";
        string _nome_cliente;

        #endregion

        List<string> Inscricoes = new List<string>();
        List<DAI> DAIList = new List<DAI>();
        List<Empresa> ListaEmpresas = new List<Empresa>();

        PhantomJSDriver _driver = null;
        PhantomJSDriver _driverTemp;
        PhantomJSDriverService service = null;
        OpenQA.Selenium.IWebElement element = null;

        public Work()
        {
            Log("############ Inicialização de automação [Tomar Ciência] ############");
            //Log(null, null, true);

            _urlApiBase = System.Configuration.ConfigurationSettings.AppSettings["ApiBaseUrl"];
        }

        public Work(int cd_bot_exec, int cd_par, string nome_cliente)
        {
            _cd_bot_exec = cd_bot_exec;
            _cd_par = cd_par;
            _nome_cliente = nome_cliente;

            Log("############ Inicialização de automação [Tomar Ciência] ############");
            //Log(null, null, true);

            _urlApiBase = System.Configuration.ConfigurationSettings.AppSettings["ApiBaseUrl"];
        }

        protected void Log(string text, string caller = "", bool newLine = false)
        {
            if (!newLine)
                LogController.RegistrarLog(_nome_cliente + " - " + String.Format("[{0}]", DateTime.Now.ToString()) + " -> " + caller + " - " + text, eTipoLog.INFO, _cd_bot_exec, "bot");
            else
                LogController.RegistrarLog(_nome_cliente + " - " + string.Empty, eTipoLog.INFO, _cd_bot_exec, "bot");
        }

        public async Task Start()
        {
            try
            {
                this.service = PhantomJSDriverService.CreateDefaultService();

                // Carrega o certificado
                ControleCertificados.CarregarCertificado(service);

                this.service.AddArgument("test-type");
                this.service.AddArgument("no-sandbox");
                this.service.HideCommandPromptWindow = true;

                this._driver = new PhantomJSDriver(service);
                this._driverTemp = new PhantomJSDriver(service);
            }
            catch
            {
                var error = true;
            }

            // Executa o passo-a-passo

            CarregaListaEmpresas();

            string urlAcompanha = _urlApiBase + $"sso/v1/parceironegocio/consulta/" + _cd_par;

            try
            {
                using (var client = new HttpClient())
                {
                    LogController.RegistrarLog(_nome_cliente + " - " + "ABRINDO CONEXAO...", eTipoLog.INFO, _cd_bot_exec, "bot");

                    var result = await client.GetAsync(urlAcompanha);
                    var registros = await result.Content.ReadAsAsync<List<ParceiroNegocio>>();

                    if (registros != null && registros.Any())
                    {
                        foreach (var item in ListaEmpresas)
                        {
                            if (item.CNPJ.Trim() == registros[0].CNPJ.Replace(".","").Replace("/", "").Replace("-", "").Trim())
                            {
                                Main(item);
                                Finish();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogController.RegistrarLog(_nome_cliente + " - " + ex.Message, eTipoLog.ERRO, _cd_bot_exec, "bot");
            }
            
        }

        protected void Finish()
        {
            this._driver.Quit();
            this.service.Dispose();
        }

        /// <summary>
        /// Carrega, do site, a lista de todas as empresas com suas respectivas Inscrições Estaduais
        /// para agilizar a navegação e manipulação dos dados.
        /// O número da Inscrição Estadual será posteriormente usado para acesso à URL "_urlIncricao".
        /// </summary>
        protected void CarregaListaEmpresas()
        {
            this._driver.Navigate().GoToUrl(_urlPrincipal);
            element = _driver.FindElementByCssSelector("#areaTrabalho > table > tbody > tr > td > table > tbody > tr:nth-child(2) > td > table > tbody > tr > td:nth-child(3) > a > img");
            element.Click();

            bool leEmpresas = true;
            bool leInscricoes = true;
            int contagemImpar = 1;
            int contagemPar = 4;
            int contagemInscricao = 1;

            while (leEmpresas)
            {
                Empresa empresa = new Empresa();

                try
                {
                    razaoSocial = this._driver.FindElement(By.CssSelector(String.Format("#areaTrabalho > table > tbody > tr > td > table:nth-child({0}) > tbody > tr:nth-child(1) > td:nth-child(3)", contagemImpar))).Text;

                    if (razaoSocial.Contains(_nome_cliente.Substring(0,10)))
                    {
                        leEmpresas = false;

                        CNPJ = this._driver.FindElement(By.CssSelector(String.Format("#areaTrabalho > table > tbody > tr > td > table:nth-child({0}) > tbody > tr:nth-child(2) > td:nth-child(3)", contagemImpar))).Text;

                        empresa.Nome = razaoSocial;
                        empresa.CNPJ = CNPJ.Replace(".", "").Replace("-", "").Replace("/", "");
                        empresa.IncricoesEstaduais = new List<string>();

                        ListaEmpresas.Add(empresa);

                        leInscricoes = true;
                        contagemInscricao = 1;

                        while (leInscricoes)
                        {
                            try
                            {
                                inscricaoEstadual = this._driver.FindElement(By.CssSelector(String.Format("#areaTrabalho > table > tbody > tr > td > table:nth-child({0}) > tbody > tr:nth-child({1}) > td.dg_item > a > b", contagemPar, contagemInscricao))).Text;
                                empresa.IncricoesEstaduais.Add(inscricaoEstadual.Replace(".", "").Replace("-", ""));

                                contagemInscricao += 1;
                            }
                            catch
                            {
                                Log(String.Format("Empresa {0}: {1} inscrição(ões) estadual(ais).", empresa.Nome, empresa.IncricoesEstaduais.Count), nameof(CarregaListaEmpresas));
                                leInscricoes = false;
                            }
                        }

                        return;
                    }

                    contagemPar += 6;
                    contagemImpar += 6;
                }
                catch
                {
                    // Não foi encontrado mais nenhuma empresa na estrutura, então sai do loop e encerra o método.
                    Log(String.Format("{0} empresas carregadas.", this.ListaEmpresas.Count));
                    leEmpresas = false;
                }
            }
        }

        // Verifica se um Alert box foi exibido
        public bool isAlertPresent()
        {
            try
            {
                this._driver.SwitchTo().Alert();
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected IWebElement FindDILink()
        {
            ReadOnlyCollection<IWebElement> elements = null;

            try
            {
                elements = this._driver.FindElements(By.TagName("a"));
                // Search for the link
                foreach (IWebElement tag in elements)
                {
                    if (tag.GetAttribute("href").Contains("consultaDIOnline.asp"))
                    {
                        Log("Link para a DI encontrado." + " " + nameof(FindDILink));
                        return tag;
                    }
                }
            }
            catch (NoSuchElementException ex)
            {
                string errorMessage = JsonConvert.DeserializeObject(ex.Message).ToString();
                Log(errorMessage + " " + nameof(FindDILink));
            }

            return null;
        }

        /// <summary>
        /// Salva o arquivo de Lacre como PDF.
        /// </summary>
        protected bool DownloadDAILacre(string numeroDI)
        {
            var retorno = false;

            ReadOnlyCollection<IWebElement> elements = null;
            try
            {
                elements = this._driver.FindElementsByClassName("link");

                foreach (IWebElement element in elements)
                {
                    if (element.Text.Contains("[Imp. de Lacre]"))
                    {
                        Log("LOCALIZA O LINK PARA DOWNLOAD....");
                        var aux = _driver.PageSource;

                        var position = aux.IndexOf("[Imp. de Lacre]");

                        var nroDI = aux.Substring((position - 693), 9);

                        if (numeroDI == nroDI)
                        {
                            var nroLacre = aux.Substring((position - 13), 7);

                            _driverTemp.Navigate().GoToUrl(_urlDocLacre + nroLacre);
                            Thread.Sleep(1000);

                            var print = CapturaImagem(_driverTemp, nroLacre);
                        }
                        else
                        {
                            nroDI = aux.Substring((position - 781), 9);

                            var nroLacre = aux.Substring((position - 13), 7);

                            _driverTemp.Navigate().GoToUrl(_urlDocLacre + nroLacre);
                            Thread.Sleep(1000);

                            var print = CapturaImagem(_driverTemp, nroLacre);
                        }

                        retorno = true;
                    }
                }

                return retorno;
            }
            catch (NoSuchElementException ex)
            {
                Log(ex.Message + " " + nameof(DownloadDAILacre));
                return false;
            }
        }

        public bool Screenshot(IWebDriver driver, string screenshotsPasta)
        {
            try
            {
                ITakesScreenshot camera = driver as ITakesScreenshot;
                Screenshot foto = camera.GetScreenshot();
                foto.SaveAsFile(screenshotsPasta + ".png", ScreenshotImageFormat.Png);

                System.Drawing.Image png = System.Drawing.Image.FromFile(screenshotsPasta + ".png");
                using (var bitmap = new Bitmap(png.Width, png.Height))
                {
                    //bitmap.SetResolution( png.HorizontalResolution, png.VerticalResolution);

                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.Clear(Color.White);
                        g.DrawImageUnscaled(png, 0, 0);
                    }

                    bitmap.Save(screenshotsPasta + ".jpg", ImageFormat.Jpeg);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool CapturaImagem(PhantomJSDriver _driver, string numero)
        {
            try
            {
                Log("CAPTURA O PRINT DA TELA DO DOCUMENTO....");
                //FUTURAMENTE ESSE CAMINHO SERÁ CONFIGURADO EM UMA TABELA
                if (!System.IO.Directory.Exists(@"C:\Versatilly\"+ _nome_cliente + "\\"))
                {
                    System.IO.Directory.CreateDirectory(@"C:\Versatilly\"+ _nome_cliente + "\\");
                }

                string arquivoPath = Path.Combine(@"C:\Versatilly\"+ _nome_cliente + "\\", numero + "-CapturaTela");

                var retornoPrint = Screenshot(_driver, arquivoPath);

                if (retornoPrint)
                {
                    Log("CONVERTE A IMAGEM .JPG EM PDF...");
                    ConvertImagePDF(numero + "-CapturaTela.jpg", numero);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool ConvertImagePDF(string nomePrint, string lacre)
        {
            try
            {
                iTextSharp.text.Document Doc = new iTextSharp.text.Document(PageSize.A3);

                //FUTURAMENTE ESSE CAMINHO SERÁ CONFIGURADO EM UMA TABELA
                if (!System.IO.Directory.Exists(@"C:\Versatilly\"+ _nome_cliente + "\\"))
                {
                    System.IO.Directory.CreateDirectory(@"C:\Versatilly\"+ _nome_cliente + "\\");
                }

                string arquivoPath = Path.Combine(@"C:\Versatilly\"+ _nome_cliente + "\\");

                string PDFOutput = Path.Combine(arquivoPath, "Lacre-" + lacre + ".pdf");
                PdfWriter writer = PdfWriter.GetInstance(Doc, new FileStream(PDFOutput, FileMode.Create, FileAccess.Write, FileShare.Read));

                Doc.Open();

                foreach (string F in System.IO.Directory.GetFiles(arquivoPath, nomePrint))
                {
                    Doc.NewPage();

                    Doc.Add(new iTextSharp.text.Jpeg(new Uri(new FileInfo(F).FullName)));
                }

                Doc.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Principal método do Robô. Aqui a sessão para cada Inscrição Estadual é estabelecida
        /// ao acessar a "_urlInscriçao"
        /// </summary>
        protected async Task Main(Empresa empTomarCiencia)
        {
            int paginaInicial = 1;
            int numeroPaginas;
            List<DAI> listaDAIProcessadas = new List<DAI>();
            int r = 0; // Índice das células do arquivo excel ( xls )
            int j = -1; // Índice das colunas do arquivo excel ( xls )

            int colDI = 0;
            int colData = 0;
            int colSinal = 0;

            ExcelPackage xlsDoc = null;
            ExcelWorksheet sheet = null;

            var drEmpresa = empTomarCiencia;

           // Log("Empresa - " + drEmpresa.Nome.Trim());

            //CRIA PLANILHA PARA SAMSUNG E VENTISOL
            if (drEmpresa.Nome.Contains("SAMSUNG") || drEmpresa.Nome.Contains("VENTISOL"))
            {
                Log("*****Criando Planilha*****");
                xlsDoc = new ExcelPackage();

                xlsDoc.Workbook.Properties.Title = "2E";

                r = 0;
                r++;
                colDI = 1;
                colData = 2;
                colSinal = 3;
            }

            foreach (string inscricao in drEmpresa.IncricoesEstaduais)
            {
                //CRIA PLANILHA PARA SAMSUNG E VENTISOL
                if (drEmpresa.Nome.Contains("SAMSUNG") || drEmpresa.Nome.Contains("VENTISOL"))
                {
                    sheet = xlsDoc.Workbook.Worksheets.Add(inscricao.Trim());
                    sheet.Name = inscricao.Trim();

                    r++;
                    sheet.Cells[r, colDI].Value = "Inscrição Nº " + inscricao.Trim();
                    r++;

                    sheet.Cells[r, colDI].Value = "Numero DI";
                    sheet.Cells[r, colData].Value = "Data";
                    sheet.Cells[r, colSinal].Value = "Canal";

                    r++;
                }

                // Garante que a sessão corrente esteja no contexto da Inscrição Estadual atual
                this._driver.Navigate().GoToUrl(this._urlIncricao + inscricao);
              //  Log(null, null, true);
                Log(String.Format("Verificando DAIs da Inscrição [{0}]", inscricao), nameof(Main));
                
                // Segue diretamente à listagem das DAI's da Inscrição Estadual
                // selecionada no passo anterior
                this._driver.Navigate().GoToUrl(this._urlConsultaDI + paginaInicial);
                Thread.Sleep(1000);

                string totalPaginas = "0";

                try
                {
                    // Captura o total de páginas da listagem de DAI's da Inscrição Estadual atual
                    totalPaginas = this._driver.FindElement(By.XPath("/html/body/table[1]/tbody/tr/td/table/tbody/tr/td[3]")).Text;
                    numeroPaginas = Convert.ToInt32(totalPaginas.Substring(totalPaginas.LastIndexOf('/') + 1).Trim());

                    
                }
                catch (Exception)
                {
                    numeroPaginas = 0;
                    string mensagem = this._driver.FindElement(By.XPath("/html/body/table/tbody/tr/td[2]/b")).Text;
                    Log("######" + mensagem + "######");
                }

                // Página a página, busca pelas DAI's que estejam pendentes de "Tomar Ciência".
                for (int pag = 1; pag <= numeroPaginas; pag++)
                { 
                    Log("Navegando para a página " + pag);
                    this._driver.Navigate().GoToUrl(this._urlConsultaDI + pag);
               
                    //Log("Loading de 5 mils.");
                    //Thread.Sleep(500);

                    r++;

                    if (drEmpresa.Nome.Contains("SAMSUNG") || drEmpresa.Nome.Contains("VENTISOL"))
                    {
                        sheet.Cells[r, colDI].Value = "Pagina " + pag;
                    }

                    r++;
                    r++;

                    ReadOnlyCollection<IWebElement> elements;
                    try
                    {
                        elements = _driver.FindElements(By.ClassName("dg_ln_impar"));

                        foreach (IWebElement element in elements)
                        {
                            if (element.Text.Contains("[Imp. de Lacre]"))
                            {
                                var retornoPrint = DownloadDAILacre(element.Text.Substring(0, 9));

                                if (retornoPrint)
                                {
                                    Log("PDF DOC. LACRE SALVO !");
                                }
                            }

                            if (drEmpresa.Nome.Contains("SAMSUNG") || drEmpresa.Nome.Contains("VENTISOL"))
                            {
                                var nroDI = element.Text.Substring(0, 9);
                                //Log("GRAVANDO DI= " + nroDI);

                                //LogController.RegistrarLog(_nome_cliente + " GRAVANDO DI - " + nroDI, eTipoLog.INFO, _cd_bot_exec, "bot");

                                var data = element.Text.Substring(10, 10);
                                var status = "";

                                sheet.Cells[r, colDI].Value = nroDI;
                                sheet.Cells[r, colData].Value = data;

                                if (element.Text.Contains("Parametriza"))
                                {
                                    status = "Pendente de parametrização";
                                    sheet.Cells[r, colSinal].Value = status;
                                }
                                else
                                {
                                    status = "Verde";
                                    sheet.Cells[r, colSinal].Value = status;

                                    using (var range = sheet.Cells[r, colSinal])
                                    {
                                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                                    }
                                }

                                r++;
                            }
                        }

                        elements = _driver.FindElements(By.ClassName("dg_ln_par"));

                        foreach (IWebElement elemento in elements)
                        {
                            if (elemento.Text.Contains("[Imp. de Lacre]"))
                            {
                                var retornoPrint = DownloadDAILacre(elemento.Text.Substring(0, 9));

                                if (retornoPrint)
                                {
                                    Log("PDF DOC. LACRE SALVO !");
                                }
                            }

                            if (drEmpresa.Nome.Contains("SAMSUNG") || drEmpresa.Nome.Contains("VENTISOL"))
                            {
                                var nroDI = elemento.Text.Substring(0, 9);
                                //Log("GRAVANDO DI= " + nroDI);
                                //LogController.RegistrarLog(_nome_cliente + " GRAVANDO DI - " + nroDI, eTipoLog.INFO, _cd_bot_exec, "bot");

                                var data = elemento.Text.Substring(10, 10);
                                var status = "";

                                sheet.Cells[r, colDI].Value = nroDI;
                                sheet.Cells[r, colData].Value = data;

                                if (elemento.Text.Contains("Parametriza"))
                                {
                                    status = "Pendente de parametrização";
                                    sheet.Cells[r, colSinal].Value = status;
                                }
                                else
                                {
                                    status = "Verde";
                                    sheet.Cells[r, colSinal].Value = status;

                                    using (var range = sheet.Cells[r, colSinal])
                                    {
                                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                                    }
                                }

                                r++;
                            }
                        }
                    }
                    catch (NoSuchElementException ex)
                    {

                    }
                }

                r = 1;
                colDI = 1;
                colData = 2;
                colSinal = 3;
            }

            if (drEmpresa.Nome.Contains("SAMSUNG") || drEmpresa.Nome.Contains("VENTISOL"))
            {
                //FUTURAMENTE ESSE CAMINHO SERÁ CONFIGURADO EM UMA TABELA
                if (!System.IO.Directory.Exists(@"C:\Versatilly\"+ _nome_cliente + "\\"))
                {
                    System.IO.Directory.CreateDirectory(@"C:\Versatilly\"+ _nome_cliente + "\\");
                }

                var horaData = DateTime.Now.ToString().Replace("/", "").Replace(":", "").Replace(" ", "");

                string arquivoPath = Path.Combine(@"C:\Versatilly\"+ _nome_cliente + "\\", horaData + "-TomarCiencia" + drEmpresa.Nome.Trim().Replace(" ", "") + ".xlsx");

                File.WriteAllBytes(arquivoPath, xlsDoc.GetAsByteArray());
            }

            var debugStop = false;
        }
    }
}

