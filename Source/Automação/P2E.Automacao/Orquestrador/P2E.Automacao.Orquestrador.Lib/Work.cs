﻿using P2E.Automacao.Entidades;
using P2E.Automacao.Orquestrador.DataContext;
using P2E.Automacao.Orquestrador.Lib.Entidades;
using P2E.Automacao.Orquestrador.Lib.Util.Extensions;
using P2E.Automacao.Orquestrador.Repositories;
using P2E.Automacao.Shared.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace P2E.Automacao.Orquestrador.Lib
{
    public class Work
    {
        protected IEnumerable<Agenda> _agendas;
       // private List<Importacao> registros;
        private List<TriagemBot> triagem;
        private string _urlApiBase;
        public Work() => _urlApiBase = System.Configuration.ConfigurationSettings.AppSettings["ApiBaseUrl"];

        public async Task ExecutarAsync()
        {
            LogController.RegistrarLog("========================================ORQUESTRADOR====================================================");
            LogController.RegistrarLog($"Execução iniciada em {DateTime.Now}");
            LogController.RegistrarLog($"-------------------------------------------------------------------------------------------------------");
            LogController.RegistrarLog($"Iniciando monitoramento--------------------------------------------------------------------------------");

            while (true)
            {
                try
                {
                    string data = DateTime.Today.ToString("dd-MM-yyyy", null);
                    _agendas = CarregarAgendasAsync().Result;

                    if (_agendas.Any())
                    {
                        LogController.RegistrarLog("Veficicando Agendamentos");

                        if (_agendas.Any(p => p.OP_STATUS == eStatusExec.Aguardando_Processamento))
                        {
                            Parallel.ForEach(_agendas.Where(p => p.OP_STATUS == eStatusExec.Aguardando_Processamento), async agenda =>
                            {
                                if (agenda.AgendaProgramada != null)
                                {
                                    if (agenda.OP_STATUS == eStatusExec.Aguardando_Processamento)
                                    {
                                        await ExecutarAgendaAsync(agenda);
                                    }
                                }
                            });
                        }
                        else
                        {
                            LogController.RegistrarLog("Nenhuma agenda para executar.");
                        }
                    }

                    Thread.Sleep(30000);
                }
                catch (Exception ex)
                {
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO);
                }
            }
        }

        /// <summary>
        /// Executar as agendas que estao com status = Programada
        /// </summary>
        /// <param name="agenda"></param>
        /// <returns></returns>
        public async Task ExecutarAgendaAsync(Agenda agenda)
        {
            LogController.RegistrarLog($"Executando agenda '{agenda.TX_DESCRICAO}'");

            await AlterarStatusAgendaAsync(agenda, eStatusExec.Executando);

            LogController.RegistrarLog($"Executando bots.", eTipoLog.INFO, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda", "");
            if (agenda.Bots != null)
            {
                foreach (var bot in agenda.Bots)
                {
                    if (bot.BotProgramado != null)
                    {
                        await AlterarStatusBotAsync(bot, eStatusExec.Executando);
                        await ExecutarBotAsync(bot);
                    }

                    if (agenda.Bots.Any(p => p.CD_ULTIMO_STATUS_EXEC_BOT == eStatusExec.Falha))
                    {
                        await AlterarStatusAgendaAsync(agenda, eStatusExec.Falha);

                        if (agenda.OP_RETENTAR == 1)
                        {
                            ProgramarAgendaAsync(agenda, eFormaExec.Automática);
                        }
                    }
                    else
                    {
                        await AlterarStatusAgendaAsync(agenda, eStatusExec.Concluído);
                        if (agenda.OP_LOOP == 1)
                        {
                            ProgramarAgendaAsync(agenda, eFormaExec.Automática);
                        }
                    }
                }
            }
        }

        public async Task AlterarStatusAgendaAsync(Agenda agenda, eStatusExec novoStatus)
        {
            using (var context = new OrquestradorContext())
            {
                var agendaRep = new AgendaRepository(context);
                var botExecRep = new BotExecRepository(context);
                var agendaExecRep = new AgendaExecRepository(context);

                if (agenda.AgendaProgramada != null)
                {
                    LogController.RegistrarLog($"Alterando status da agenda ['{agenda.TX_DESCRICAO}'] de ['{agenda.OP_STATUS.GetDescription()}'] " +
                           $"para ['{novoStatus.GetDescription()}']", eTipoLog.INFO, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda", "");


                    agenda.AgendaProgramada.OP_STATUS_AGENDA_EXEC = novoStatus;
                    agenda.OP_STATUS = novoStatus;

                    if (novoStatus == eStatusExec.Executando)
                    {
                        agenda.AgendaProgramada.DT_INICIO_EXEC = DateTime.Now;
                        agenda.DT_DATA_INICIO_ULTIMA_EXEC = agenda.AgendaProgramada.DT_INICIO_EXEC;
                        agenda.DT_DATA_FIM_ULTIMA_EXEC = null;
                        agenda.CD_ULTIMA_EXEC = agenda.AgendaProgramada.CD_AGENDA_EXEC;
                    }

                    if (novoStatus == eStatusExec.Falha || novoStatus == eStatusExec.Concluído)
                    {
                        agenda.AgendaProgramada.DT_FIM_EXEC = DateTime.Now;
                        agenda.CD_ULTIMA_EXEC = agenda.AgendaProgramada.CD_AGENDA_EXEC;
                        agenda.DT_DATA_FIM_ULTIMA_EXEC = agenda.AgendaProgramada.DT_FIM_EXEC;
                    }

                    await agendaExecRep.UpdateAsync(agenda.AgendaProgramada);
                    await agendaRep.UpdateAsync(agenda);
                }
            }
        }
        public async Task AlterarStatusBotAsync(AgendaBot bot, eStatusExec novoStatus)
        {
            if (bot.BotProgramado != null)
            {
                LogController.RegistrarLog($"alterando status do bot '{bot.Bot.TX_DESCRICAO}' de [{bot.BotProgramado.OP_STATUS_BOT_EXEC.GetDescription()}] " +
                       $"para ['{novoStatus.GetDescription()}']", eTipoLog.INFO, bot.BotProgramado.CD_BOT_EXEC, "bot", "");

                using (var context = new OrquestradorContext())
                {
                    var agendaBotRep = new AgendaBotRepository(context);
                    var botExecRep = new BotExecRepository(context);

                    bot.BotProgramado.OP_STATUS_BOT_EXEC = novoStatus;
                    bot.UltimoBotExec = bot.BotProgramado;

                    if (novoStatus == eStatusExec.Executando)
                    {
                        bot.BotProgramado.DT_INICIO_EXEC = DateTime.Now;
                    }

                    

                    if (novoStatus == eStatusExec.Falha || novoStatus == eStatusExec.Concluído)
                    {
                        bot.BotProgramado.DT_FIM_EXEC = DateTime.Now;

                        bot.CD_ULTIMA_EXEC_BOT = bot.BotProgramado.CD_BOT_EXEC;
                        bot.CD_ULTIMO_STATUS_EXEC_BOT = bot.BotProgramado.OP_STATUS_BOT_EXEC;
                    }

                    await botExecRep.UpdateAsync(bot.BotProgramado);
                    await agendaBotRep.UpdateAsync(bot);
                }
            }
        }

        private async Task ExecutarBotAsync(AgendaBot bot)
        {
            try
            {
                string urlParceirosNegocio = _urlApiBase + $"imp/v1/triagembot/cd_par";

                using (var client = new HttpClient())
                {
                    var result = await client.GetAsync(urlParceirosNegocio);
                    triagem = await result.Content.ReadAsAsync<List<TriagemBot>>();
                    string nomeCliente = "";

                    Parallel.ForEach(triagem, item =>
                    {
                        string urlAcompanha = _urlApiBase + $"sso/v1/parceironegocio/consulta/" + item.CD_PAR_NEG;

                        var resultadox = client.GetAsync(urlAcompanha).Result;
                        var clientes = resultadox.Content.ReadAsAsync<List<ParceiroNegocio>>();

                        if (clientes != null)
                        {
                            nomeCliente = clientes.Result[0].TXT_RZSOC.Trim();
                        }



                        switch (bot.Bot.TX_NOME.ToUpper())
                        {
                            case "ROBÔ 01":
                                RoboBaixarExtato(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 02":
                                RoboAcompanharDespacho(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 03":
                                RoboComprovanteImportacao(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 04":
                                RoboExonerarICMS(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 05":
                                RoboExtratoRetificacao(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 06":
                                RoboTelaDebito(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 07":
                                break;
                            case "ROBÔ 08":
                                break;
                            case "ROBÔ 09":
                                RoboTomarCiencia(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 10":
                                break;
                            case "ROBÔ 11":
                                break;
                            case "ROBÔ 12":
                                RoboStatusDesembaracoSefaz(bot, item, nomeCliente);
                                break;
                            case "ROBÔ 15":
                                break;
                        }
                    });

                    switch (bot.Bot.TX_NOME.ToUpper())
                    {
                        case "ROBÔ 08":
                            RoboTaxaCambio(bot);
                            break;
                        case "ROBÔ 15":
                            RoboAtualizaListaSuframa(bot);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogController.RegistrarLog($"Erro em [ExecutarBotAsync]. {ex.Message}", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
            }
        }

        private void RoboAtualizaListaSuframa(AgendaBot bot)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new P2E.Automacao.Processos.AtualizaListaSuframa.Lib.Work(bot.BotProgramado.CD_BOT_EXEC).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboStatusDesembaracoSefaz(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new P2E.Automacao.Processos.StatusDesembaracoSefaz.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboTomarCiencia(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {

                    new TomarCiencia.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).Start();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboTaxaCambio(AgendaBot bot)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new Processos.TaxaConversaoCambio.Lib.Work().ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboTelaDebito(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new Processos.TelaDebito.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboExtratoRetificacao(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new Processos.ExtratoRetificacao.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboExonerarICMS(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    new ExonerarIcms.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboComprovanteImportacao(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new Processos.ComprovanteImportacao.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboAcompanharDespacho(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await new Processos.AcompanharDespachos.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        private void RoboBaixarExtato(AgendaBot bot, TriagemBot item, string nomeCliente)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    Task task = new BaixarExtratos.Lib.Work(bot.BotProgramado.CD_BOT_EXEC, item.CD_PAR_NEG, nomeCliente).ExecutarAsync();
                    task.Wait();

                    await AlterarStatusBotAsync(bot, eStatusExec.Concluído);
                }
                catch (ThreadAbortException abort)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Interrompido);
                    LogController.RegistrarLog($"Execução interrompida.", eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
                catch (Exception ex)
                {
                    await AlterarStatusBotAsync(bot, eStatusExec.Falha);
                    LogController.RegistrarLog(ex.Message, eTipoLog.ERRO, bot.BotProgramado.CD_BOT_EXEC, "bot");
                }
            });
        }

        /// <summary>
        /// Carregar as agendas para execução
        /// </summary>
        /// <returns></returns>
        private async Task<List<Agenda>> CarregarAgendasAsync()
        {
            try
            {
                var agendas = new List<Agenda>();

                LogController.RegistrarLog($"-------------------------------------------------------------------------------------------------------");
                LogController.RegistrarLog("Carregando Agendas.");
                using (var context = new OrquestradorContext())
                {
                    var agendaRep = new AgendaRepository(context);
                    var agendaBotRep = new AgendaBotRepository(context);
                    var agendaExecRep = new AgendaExecRepository(context);
                    var botRep = new BotRepository(context);
                    var botExecRep = new BotExecRepository(context);

                    LogController.RegistrarLog($"Obtendo agendas ativas.");

                    agendas = agendaRep.FindAll(o => o.OP_ATIVO == 1).ToList();

                    if (agendas.Any())
                    {
                        LogController.RegistrarLog($"{agendas.Count()} agenda(s) localizada(s).");

                        foreach (var agenda in agendas)
                        {

                            if (agenda.OP_FORMA_EXEC == eFormaExec.Automática && agenda.OP_STATUS != eStatusExec.Aguardando_Processamento && agenda.OP_STATUS != eStatusExec.Executando && agenda.OP_STATUS != eStatusExec.Programado)
                            {
                                ProgramarAgendaAsync(agenda, eFormaExec.Automática);
                            }
                            else if (agenda.OP_STATUS == eStatusExec.Programado)
                            {
                                LogController.RegistrarLog($"Carregando os bots associados a agenda '{agenda.TX_DESCRICAO}.'");

                                if (VerificaProgramacaoAgenda(agenda))
                                {
                                    await CarregarBotsAsync(agenda);

                                    if (agenda.Bots != null && agenda.Bots.Any())
                                    {
                                        LogController.RegistrarLog($"{agenda.Bots.Count()} bots encontrados.", eTipoLog.INFO, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda");


                                        await AlterarStatusAgendaAsync(agenda, eStatusExec.Aguardando_Processamento);
                                    }
                                    else
                                    {
                                        LogController.RegistrarLog($"Nenhum bot localizado para a agenda '{agenda.TX_DESCRICAO}.'");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        LogController.RegistrarLog("Nenhuma programação localizada.");
                    }
                }

                LogController.RegistrarLog($"-------------------------------------------------------------------------------------------------------");

                //await Task.Delay(TimeSpan.FromSeconds(5));
                Thread.Sleep(5000);
                return agendas;
            }
            catch (Exception ex)
            {
                LogController.RegistrarLog($"Erro em CarregarAgendasAsync. {ex.Message}");
                return null;
            }
        }

        public List<Agenda> CarregarProgramacaoAsync()
        {
            try
            {
                var agendas = new List<Agenda>();
                using (var context = new OrquestradorContext())
                {
                    var agendaRep = new AgendaRepository(context);
                    var agendaBotRep = new AgendaBotRepository(context);
                    var agendaExecRep = new AgendaExecRepository(context);
                    var botRep = new BotRepository(context);
                    var botExecRep = new BotExecRepository(context);

                    agendas = agendaRep.FindAll().ToList();

                    if (agendas.Any())
                    {
                        foreach (var agenda in agendas)
                        {
                            // carregar ultima execução Concluída / Com falha / Interrompida.
                            agenda.UltimaAgendaExecutada = agendaExecRep.FindAll(p =>
                                p.CD_AGENDA == agenda.CD_AGENDA
                                ).OrderByDescending(o=> o.CD_AGENDA_EXEC).Where(a =>
                                    a.OP_STATUS_AGENDA_EXEC == eStatusExec.Concluído
                                 || a.OP_STATUS_AGENDA_EXEC == eStatusExec.Falha
                                 || a.OP_STATUS_AGENDA_EXEC == eStatusExec.Interrompido).FirstOrDefault();


                            if (agenda.OP_STATUS == eStatusExec.Programado || agenda.OP_STATUS == eStatusExec.Aguardando_Processamento || agenda.OP_STATUS == eStatusExec.Executando || agenda.OP_STATUS == eStatusExec.Retentar)
                            {
                                agenda.AgendaProgramada = agendaExecRep.Find(p => p.CD_AGENDA_EXEC == agenda.CD_ULTIMA_EXEC);
                            }

                            agenda.Bots = agendaBotRep.FindAll(p => p.CD_AGENDA == agenda.CD_AGENDA);

                            foreach (var bot in agenda.Bots)
                            {
                                bot.Bot = botRep.Find(o => o.CD_BOT == bot.CD_BOT);
                                if (agenda.OP_STATUS == eStatusExec.Programado)
                                {
                                    bot.BotProgramado = botExecRep.Find(o => o.CD_BOT == bot.CD_BOT && o.CD_AGENDA_EXEC == agenda.AgendaProgramada.CD_AGENDA_EXEC);
                                }
                            }
                        }
                    }
                }
                return agendas;
            }
            catch (Exception ex)
            {
                LogController.RegistrarLog($"Erro em CarregarAgendasAsync. {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Carregar bots associados a agenda
        /// </summary>
        /// <param name="agenda"></param>
        /// <returns></returns>
        private async Task CarregarBotsAsync(Agenda agenda)
        {
            try
            {
                using (var context = new OrquestradorContext())
                {
                    var agendaBotRep = new AgendaBotRepository(context);
                    var botExecRep = new BotExecRepository(context);
                    var botRep = new BotRepository(context);

                    agenda.Bots = agendaBotRep.FindAll(p => p.CD_AGENDA == agenda.CD_AGENDA);


                    foreach (var bot in agenda.Bots)
                    {
                        bot.Bot = await botRep.FindAsync(o => o.CD_BOT == bot.CD_BOT);
                        if (agenda.OP_STATUS == eStatusExec.Programado && agenda.AgendaProgramada != null)
                        {
                            bot.BotProgramado = await botExecRep.FindAsync(o => o.CD_BOT == bot.CD_BOT && o.CD_AGENDA_EXEC == agenda.AgendaProgramada.CD_AGENDA_EXEC);
                            await AlterarStatusBotAsync(bot, eStatusExec.Aguardando_Processamento);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogController.RegistrarLog($"Erro em [CarregarBotsAsync]. {ex.Message}", eTipoLog.ERRO, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda");
            }
        }

        /// <summary>
        /// verifica se a agenda está na hora de executar
        /// </summary>
        /// <param name="agenda"></param>
        /// <returns></returns>
        private bool VerificaProgramacaoAgenda(Agenda agenda)
        {
            try
            {
                using (var context = new OrquestradorContext())
                {
                    var agendaBotRep = new AgendaBotRepository(context);
                    var agendaRep = new AgendaRepository(context);
                    var agendaExecRep = new AgendaExecRepository(context);
                    var botExecRep = new BotExecRepository(context);

                    if (agenda.OP_STATUS == eStatusExec.Programado || agenda.OP_STATUS == eStatusExec.Aguardando_Processamento)
                    {
                        agenda.AgendaProgramada = agendaExecRep.Find(p => p.CD_AGENDA == agenda.CD_AGENDA && (p.OP_STATUS_AGENDA_EXEC == eStatusExec.Programado));
                    }

                    // se não estiver programado, não faz nada.
                    if (agenda.OP_STATUS != eStatusExec.Programado)
                    {
                        return false;
                    }

                    // se for programadado e a execução for manual, não aguarda o tempo mínimo e já põe na fila para processar
                    if (agenda.OP_FORMA_EXEC == eFormaExec.Manual)
                    {
                        return true;
                    }

                    // se não for de repetição e a data programada for igual a hoje
                    if (agenda.DT_DATA_EXEC_PROG.HasValue && agenda.DT_DATA_EXEC_PROG.Value == DateTime.Today)
                    {
                        // se extá ou já passou da hora de execução do dia programado.
                        if (agenda.HR_HORA_EXEC_PROG <= new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second))
                        {
                            return true;
                        }
                    }

                    if (agenda.OP_REPETE == 1 && agenda.OP_FORMA_EXEC == eFormaExec.Automática)
                    {
                        switch (agenda.OP_TIPO_REP)
                        {
                            case eTipoRepete.Horário:
                                if (!agenda.DT_DATA_FIM_ULTIMA_EXEC.HasValue)
                                {
                                    if (agenda.HR_HORA_EXEC_PROG <= new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second))
                                    {
                                        return true;
                                    }
                                }
                                else
                                if (DateTime.Now.Subtract(agenda.DT_DATA_FIM_ULTIMA_EXEC.Value).TotalMinutes > agenda.HR_HORA_EXEC_PROG.TotalMinutes)
                                {
                                    return true;
                                }
                                break;
                            case eTipoRepete.Diário:
                                if (!agenda.DT_DATA_FIM_ULTIMA_EXEC.HasValue)
                                {
                                    if (agenda.HR_HORA_EXEC_PROG <= new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second))
                                    {
                                        return true;
                                    }
                                }
                                else
                                if (DateTime.Now.Subtract(agenda.DT_DATA_FIM_ULTIMA_EXEC.Value).TotalDays > 1)
                                {
                                    return true;
                                }
                                break;
                            case eTipoRepete.Semanal:
                                if (!agenda.DT_DATA_FIM_ULTIMA_EXEC.HasValue)
                                {
                                    return true;
                                }
                                else
                                if (DateTime.Now.Subtract(agenda.DT_DATA_FIM_ULTIMA_EXEC.Value).TotalDays > 7)
                                {
                                    return true;
                                }
                                break;
                            case eTipoRepete.Mensal:
                                if (!agenda.DT_DATA_FIM_ULTIMA_EXEC.HasValue)
                                {
                                    return true;
                                }
                                else
                                 if (DateTime.Now.Subtract(agenda.DT_DATA_FIM_ULTIMA_EXEC.Value).TotalDays > 30)
                                {
                                    return true;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogController.RegistrarLog($"Erro em VerificaProgramacaoAgenda. {ex.Message}", eTipoLog.ERRO, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda");
            }

            LogController.RegistrarLog($"Agenda {agenda.TX_DESCRICAO} fora da hora de execução.", eTipoLog.ALERTA, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda");
            return false;
        }

        public async Task ProgramarAgendaAsync(Agenda agenda, eFormaExec formaExec)
        {
            try
            {
                using (var context = new OrquestradorContext())
                {
                    var agendaBotRep = new AgendaBotRepository(context);
                    var agendaRep = new AgendaRepository(context);
                    var agendaExecRep = new AgendaExecRepository(context);
                    var botExecRep = new BotExecRepository(context);
                    {
                        switch (agenda.OP_STATUS)
                        {
                            case eStatusExec.Falha:
                            case eStatusExec.Interrompido:
                            case eStatusExec.Concluído:
                            case eStatusExec.Não_Programado:
                                {
                                    agenda.AgendaProgramada = new AgendaExec()
                                    {
                                        CD_AGENDA = agenda.CD_AGENDA,
                                        OP_STATUS_AGENDA_EXEC = eStatusExec.Programado
                                    };

                                    agendaExecRep.Insert(agenda.AgendaProgramada);

                                    agenda.CD_ULTIMA_EXEC = agenda.AgendaProgramada.CD_AGENDA_EXEC;
                                    agenda.OP_STATUS = agenda.AgendaProgramada.OP_STATUS_AGENDA_EXEC;
                                    agenda.OP_FORMA_EXEC = formaExec;
                                    agendaRep.Update(agenda);

                                    LogController.RegistrarLog($"Programando agenda [{agenda.TX_DESCRICAO}]", eTipoLog.INFO, agenda.AgendaProgramada.CD_AGENDA_EXEC, "agenda");


                                    IEnumerable<AgendaBot> bots = agendaBotRep.FindAll(p => p.CD_AGENDA == agenda.CD_AGENDA);
                                    if (bots != null)
                                    {
                                        foreach (var bot in bots)
                                        {
                                            bot.BotProgramado = new BotExec()
                                            {
                                                CD_AGENDA_EXEC = agenda.AgendaProgramada.CD_AGENDA_EXEC,
                                                NR_ORDEM_EXEC = bot.NR_ORDEM_EXEC,
                                                OP_STATUS_BOT_EXEC = eStatusExec.Programado,
                                                CD_BOT = bot.CD_BOT,
                                            };

                                            botExecRep.Insert(bot.BotProgramado);
                                            bot.CD_ULTIMA_EXEC_BOT = bot.BotProgramado.CD_BOT_EXEC;
                                            bot.CD_ULTIMO_STATUS_EXEC_BOT = bot.BotProgramado.OP_STATUS_BOT_EXEC;
                                            agendaBotRep.Update(bot);
                                        }
                                    }

                                    break;
                                }

                            case eStatusExec.Programado:
                                await AlterarStatusAgendaAsync(agenda, eStatusExec.Aguardando_Processamento).ConfigureAwait(false);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogController.RegistrarLog($"Erro ao programar agenda. {ex.Message}");
            }
        }

        public List<AgendaExecLog> ObterAgendaExecLogs(int cdAgendaExec)
        {
            using (var context = new OrquestradorContext())
            {
                var agendaExecLogRep = new AgendaExecLogRepository(context);
                return agendaExecLogRep.FindAll(p => p.CD_AGENDA_EXEC == cdAgendaExec).ToList();
            }
        }

        public List<BotExecLog> ObterLogsExecLogs(int cd_bot_exec)
        {
            using (var context = new OrquestradorContext())
            {
                var logRep = new BotExecLogRepository(context);
                return logRep.FindAll(p => p.CD_BOT_EXEC == cd_bot_exec).ToList();
            }
        }
    }
}
