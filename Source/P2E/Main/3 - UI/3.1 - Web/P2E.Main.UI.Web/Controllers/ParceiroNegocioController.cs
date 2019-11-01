﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using P2E.Main.UI.Web.Extensions.Alerts;
using P2E.Main.UI.Web.Extensions.Filters;
using P2E.Main.UI.Web.Extensions.Util;
using P2E.Main.UI.Web.Models;
using P2E.Main.UI.Web.Models.SSO.ParceiroNegocio;
using P2E.Shared.Message;
using P2E.Shared.Model;
using P2E.SSO.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace P2E.Main.UI.Web.Controllers
{
    [Authorize]
    public class ParceiroNegocioController : Controller
    {
        #region variáveis locais
        private readonly AppSettings appSettings;
        private readonly IMapper _mapper;
        private string _urlParceiro;
        #endregion

        #region construtor
        public ParceiroNegocioController(AppSettings appSettings, IMapper mapper)
        {
            this.appSettings = appSettings;
            _mapper = mapper;
            _urlParceiro = this.appSettings.ApiBaseURL + $"sso/v1/parceironegocio";



        }
        #endregion

        #region Métodos

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataPage"></param>
        /// <param name="razaosocial"></param>
        /// <param name="cnpj"></param>
        /// <returns></returns>
        [HttpGet]
        [PermissaoFilter("Parceiro de Negócio", "Consultar")]
        public async Task<IActionResult> Index([FromQuery] ParceiroNegocioListViewModel vm)
        {
            try
            {
                if (vm.DataPage.CurrentPage > 0)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                        var result = await client.GetAsync($"{_urlParceiro}?currentpage={vm.DataPage.CurrentPage}&pagesize={vm.DataPage.PageSize}&orderby={vm.DataPage.OrderBy}&Descending={vm.DataPage.Descending}&cnpj={vm.cnpj}&razaosocial={vm.razaosocial}");
                        result.EnsureSuccessStatusCode();
                        vm.DataPage = await result.Content.ReadAsAsync<DataPage<ParceiroNegocio>>();
                        vm.DataPage.UrlSearch = $"parceironegocio?";

                        return View("Index", vm);
                    }
                }
                if (vm.DataPage.Items.Any())
                {
                    return View("Index", vm);
                }
                else
                {
                    return View("Index", vm).WithInfo("", GenericMessages.ListNull());
                }
            }
            catch (Exception ex)
            {
                vm.DataPage.Message = ex.Message;
                return View(vm.DataPage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [PermissaoFilter("Parceiro de Negócio", "Editar")]
        public async Task<IActionResult> Edit(long id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                    var result = await client.GetAsync($"{_urlParceiro}/{id}");
                    result.EnsureSuccessStatusCode();
                    var parceiroNegocio = await result.Content.ReadAsAsync<ParceiroNegocio>();
                    var parceiroNegocioViewModel = _mapper.Map<ParceiroNegocioViewModel>(parceiroNegocio);
                    parceiroNegocioViewModel.Modulos = CarregarModulos().Result;
                    parceiroNegocioViewModel.Servicos = CarregarServicos().Result;

                    return View("Form", parceiroNegocioViewModel);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [PermissaoFilter("Parceiro de Negócio", "Visualizar")]
        public async Task<IActionResult> View(long id)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                    var result = await client.GetAsync($"{_urlParceiro}/{id}");
                    result.EnsureSuccessStatusCode();
                    var parceiroNegocio = await result.Content.ReadAsAsync<ParceiroNegocio>();
                    var parceiroNegocioViewModel = _mapper.Map<ParceiroNegocioViewModel>(parceiroNegocio);
                    parceiroNegocioViewModel.Modulos = CarregarModulos().Result;
                    parceiroNegocioViewModel.Servicos = CarregarServicos().Result;

                    return View("View", parceiroNegocioViewModel);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [PermissaoFilter("Parceiro de Negócio", "Criar")]
        public IActionResult Create()
        {
            var vm = new ParceiroNegocioViewModel();
            vm.Modulos = CarregarModulos().Result;
            vm.Servicos = CarregarServicos().Result;
            return View("Form", vm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Save(ParceiroNegocioViewModel itemViewModel)
        {
            var result = new HttpResponseMessage();
            string responseBody = string.Empty;
            try
            {
                var parceiroNegocio = _mapper.Map<ParceiroNegocio>(itemViewModel);
                if (parceiroNegocio.IsValid())
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                        result = await client.PutAsJsonAsync($"{_urlParceiro}/{parceiroNegocio.CD_PAR}", parceiroNegocio);
                        responseBody = await result.Content.ReadAsStringAsync();
                        result.EnsureSuccessStatusCode();
                        return RedirectToAction("Index").WithSuccess("Sucesso", GenericMessages.SucessSave("Parceiro Negócio"));
                    }
                }
                else
                {
                    itemViewModel.Modulos = CarregarModulos().Result;
                    itemViewModel.Servicos = CarregarServicos().Result;

                    int i = 0;
                    foreach (var pns in itemViewModel.ParceiroNegocioServicoModulo)
                    {
                        itemViewModel.ParceiroNegocioServicoModulo[i].Modulo = itemViewModel.Modulos.Find(p => p.CD_MOD == pns.CD_MOD);
                        itemViewModel.ParceiroNegocioServicoModulo[i].Servico = itemViewModel.Servicos.Find(p => p.CD_SRV == pns.CD_SRV);
                        i++;
                    }

                    return View("Form", itemViewModel).WithDanger("Erro.", GenericMessages.ErrorSave("Parceiro Negocio", parceiroNegocio.Messages));
                }
            }
            catch (Exception ex)
            {

                itemViewModel.Modulos = CarregarModulos().Result;
                itemViewModel.Servicos = CarregarServicos().Result;

                int i = 0;
                foreach (var pns in itemViewModel.ParceiroNegocioServicoModulo)
                {
                    itemViewModel.ParceiroNegocioServicoModulo[i].Modulo = itemViewModel.Modulos.Find(p => p.CD_MOD == pns.CD_MOD);
                    itemViewModel.ParceiroNegocioServicoModulo[i].Servico = itemViewModel.Servicos.Find(p => p.CD_SRV == pns.CD_SRV);
                    i++;
                }

                return View("Form", itemViewModel).WithDanger("Erro", responseBody);
            }
        }

        public IActionResult CancelEdit()
        {
            return RedirectToAction("Index").WithSuccess("Cancelada.", GenericMessages.EditCancel("Parceiro Negócio"));
        }

        public IActionResult CancelInsert()
        {
            return RedirectToAction("Index").WithSuccess("Cancelada.", GenericMessages.InsertCancel("Parceiro Negócio"));
        }

        public IActionResult CancelView()
        {
            return RedirectToAction("Index").WithSuccess("Cancelada.", GenericMessages.ShowDetailCancel("Parceiro Negócio"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// 
        [PermissaoFilter("Parceiro de Negócio", "Deletar")]
        public async Task<IActionResult> Delete(long Id)
        {
            HttpResponseMessage result = new HttpResponseMessage();
            string responseBody = string.Empty;

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                    result = await client.DeleteAsync($"{_urlParceiro}/{Id}");
                    responseBody = await result.Content.ReadAsStringAsync();
                    result.EnsureSuccessStatusCode();

                    return RedirectToAction("Index").WithSuccess("Sucesso.", GenericMessages.SucessRemove("Parceiro Negócio"));
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Edit", new { id = Id }).WithWarning("Erro.", responseBody);
            }

            //using (var client = new HttpClient())
            //{
            //    await client.DeleteAsync($"{_urlParceiro}/{Id}");
            //    return RedirectToAction("Index").WithSuccess("Sucesso.", GenericMessages.SucessRemove("Parceiro Negócio"));
            //}
        }
        #endregion

        private async Task<List<Modulo>> CarregarModulos()
        {
            string urlModulo = this.appSettings.ApiBaseURL + $"sso/v1/modulo/todos";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                var result = await client.GetAsync(urlModulo);
                var lista = await result.Content.ReadAsAsync<List<Modulo>>();
                return lista;
            }
        }

        private async Task<List<Servico>> CarregarServicos()
        {
            string urlServico = this.appSettings.ApiBaseURL + $"sso/v1/servico/todos";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.FindFirst("api_token").Value);
                var result = await client.GetAsync(urlServico);
                var lista = await result.Content.ReadAsAsync<List<Servico>>();
                return lista;
            }
        }


    }
}