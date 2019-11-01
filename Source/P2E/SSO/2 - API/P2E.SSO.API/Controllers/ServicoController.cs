﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P2E.Shared.Model;
using P2E.SSO.Domain.Entities;
using P2E.SSO.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace P2E.SSO.API.Controllers
{
    [Authorize]
    [ApiController]
    public class ServicoController : ControllerBase
    {
        private readonly IServicoRepository _servicoRepository;
        private readonly IRotinaRepository _rotinaRepository;
        private readonly IRotinaServicoRepository _rotinaServicoRepository;
        private readonly IParceiroNegocioModuloRepository _parceiroNegocioModuloRepository;

        public ServicoController(IServicoRepository servicoRepository, IRotinaServicoRepository rotinaServicoRepository, IRotinaRepository rotinaRepository, IParceiroNegocioModuloRepository parceiroNegocioModuloRepository)
        {
            _servicoRepository = servicoRepository;
            _rotinaServicoRepository = rotinaServicoRepository;
            _rotinaRepository = rotinaRepository;
            _parceiroNegocioModuloRepository = parceiroNegocioModuloRepository;
        }

        // GET: api/Modulo
        [HttpGet]
        [Route("api/v1/servico/todos")]
        public IEnumerable<Servico> Get()
        {
            var result = _servicoRepository.FindAll();
            return result;
        }

        // GET: api/Servico
        [HttpGet]
        [Route("api/v1/servico/")]
        public DataPage<Servico> Get([FromQuery] string txt_dec, [FromQuery] DataPage<Servico> page)
        {
            page = _servicoRepository.GetByPage(page, txt_dec);
            return page;
        }

        // GET: api/Servico/5
        [HttpGet]
        [Route("api/v1/servico/{id}")]
        public Servico Get(long id)
        {
            return _servicoRepository.Find(p => p.CD_SRV == id);
        }

        // POST: api/Servico
        [HttpPost]
        [Route("api/v1/servico")]
        public object Post([FromBody] Servico item)
        {
            try
            {
                if (item.IsValid() && _servicoRepository.ValidarDuplicidades(item))
                {
                    _servicoRepository.Insert(item);
                    return new { message = "OK" };
                }
                else
                {
                    return new { message = item.Notifications.FirstOrDefault().Message };
                }

            }
            catch (Exception ex)
            {
                return new { message = "Error." + ex.Message };
            }
        }

        // PUT: api/Servico/5
        [HttpPut("api/v1/servico/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Put(int id, [FromBody] Servico item)
        {
            try
            {
                if (item.IsValid() && _servicoRepository.ValidarDuplicidades(item))
                {
                    if (id > 0)
                        _servicoRepository.Update(item);
                    else
                        _servicoRepository.Insert(item);

                    return Ok(item);
                }
                else
                {
                    return BadRequest(item.Messages);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("api/v1/servico/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var objeto = _servicoRepository.FindById(id);
                var rotinas = _rotinaRepository.Find(p => p.CD_SRV == id);
                var parceiro = _parceiroNegocioModuloRepository.Find(p => p.CD_SRV == id);

                if (parceiro != null)
                {
                    return BadRequest("Não foi possivel excluir esse serviço pois ele está associado a um Parceiro Negócio.");
                }

                if (rotinas != null)
                {
                    return BadRequest("Não foi possivel excluir esse serviço pois ele está associado a alguma rotina.");
                }
                else
                {
                    _servicoRepository.Delete(objeto);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao tentar excluir o registro. {ex.Message}");
            }

            //try
            //{
            //    var objeto = _servicoRepository.FindById(id);
            //    _servicoRepository.Delete(objeto);
            //    return new { message = "OK" };
            //}
            //catch (Exception ex)
            //{
            //    return new { message = "Error." + ex.Message };
            //}
        }
    }
}
