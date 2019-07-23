﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using P2E.Main.API.ViewModel;
using P2E.Main.UI.Web.Extensions.Alerts;
using P2E.Main.UI.Web.Models;

namespace P2E.Main.UI.Web.Controllers
{
    public class ExemploController : Controller
    {
        private readonly AppSettings appSettings;

        public ExemploController(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            HttpClient client = new HttpClient();
            var result = await client.GetAsync(this.appSettings.ApiBaseURL+"/exemplo");
            result.EnsureSuccessStatusCode();
            List<ExemploVM> list = await result.Content.ReadAsAsync<List<ExemploVM>>();
            
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Cadastro(int id)
        {
            if(id != 0)
            {
                HttpClient client = new HttpClient();
                var result = await client.GetAsync(this.appSettings.ApiBaseURL + "/exemplo/"+id);
                result.EnsureSuccessStatusCode();

                ExemploVM exemplo = await result.Content.ReadAsAsync<ExemploVM>();

                return View(exemplo);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastro(ExemploVM exemplo)
        {
            if(exemplo.Descricao == String.Empty || exemplo.Valor <= 0)
            {
                return View(exemplo).WithDanger("Erro.", "Preencha todos os campos.");
            }

            HttpClient client = new HttpClient();
            await client.PutAsJsonAsync<ExemploVM>(this.appSettings.ApiBaseURL + "/exemplo/"+exemplo.ExemploId , exemplo);
            return RedirectToAction("Lista").WithSuccess("Sucesso.", "O Exemplo foi salvo corretamente."); ;
        }
    }
}