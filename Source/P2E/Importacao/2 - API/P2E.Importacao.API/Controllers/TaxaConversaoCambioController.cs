﻿using Microsoft.AspNetCore.Mvc;
using P2E.Importacao.Domain.Entities;
using P2E.Importacao.Domain.Repositories;
using P2E.Shared.Model;
using System;
using System.Collections.Generic;
using System.Net;

namespace P2E.Importacao.API.Controllers
{
    public class TaxaConversaoCambioController : ControllerBase
    {
        private readonly ITaxaConversaoCambioRepository _taxaConversaoCambioRepository;

        public TaxaConversaoCambioController(ITaxaConversaoCambioRepository taxaConversaoCambioRepository)
        {
            _taxaConversaoCambioRepository = taxaConversaoCambioRepository;
        }

        // GET: api/TaxaConversaoCambio
        [HttpGet]
        [Route("api/v1/taxa/moeda/{id}")]
        public IEnumerable<TaxaConversaoCambio> ListMoeda(string id)
        {
            var result = _taxaConversaoCambioRepository.FindAll(i => i.TX_MOEDA == id);
            return result;
        }

        // GET: api/TaxaConversaoCambio
        [HttpGet]
        [Route("api/v1/taxa/todos")]
        public IEnumerable<TaxaConversaoCambio> Get()
        {
            var result = _taxaConversaoCambioRepository.FindAll();
            return result;
        }

        // GET: api/TaxaConversaoCambio
        [HttpGet]
        [Route("api/v1/taxa/")]
        public DataPage<TaxaConversaoCambio> Get([FromQuery] string tx_dsc, [FromQuery] DataPage<TaxaConversaoCambio> page)
        {
            page = _taxaConversaoCambioRepository.GetByPage(page, tx_dsc);

            return page;
        }

        // GET: api/TaxaConversaoCambio/5
        [HttpGet]
        [Route("api/v1/taxa/{id}")]
        public TaxaConversaoCambio Get(long id)
        {
            TaxaConversaoCambio TaxaConversaoCambio = _taxaConversaoCambioRepository.Find(p => p.CD_TAXA_CAMBIO == id);

            return TaxaConversaoCambio;
        }

        // POST: api/TaxaConversaoCambio
        [HttpPost]
        [Route("api/v1/taxa")]
        public object Post([FromBody] TaxaConversaoCambio item)
        {
            try
            {
                _taxaConversaoCambioRepository.Insert(item);
                return new { message = "OK" };
            }
            catch (Exception ex)
            {
                return new { message = "Error." + ex.Message };
            }
        }

        // PUT: api/TaxaConversaoCambio/5
        [HttpPut]
        [Route("api/v1/taxa/{id}")]
        public IActionResult Put(int id, [FromBody] TaxaConversaoCambio item)
        {
            try
            {
                if (id > 0)
                    _taxaConversaoCambioRepository.Update(item);
                else
                    _taxaConversaoCambioRepository.Insert(item);

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.Message);
                // return new { message = "Error." + ex.Message };
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        [Route("api/v1/taxa/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var objeto = _taxaConversaoCambioRepository.FindById(id);

                _taxaConversaoCambioRepository.Delete(objeto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao tentar excluir o registro. {ex.Message}");
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete]
        [Route("api/v1/taxa/deleteall")]
        public IActionResult DeleteAll()
        {
            try
            {
                _taxaConversaoCambioRepository.DeleteAll();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao tentar excluir os registros. {ex.Message}");
            }
        }
    }
}
