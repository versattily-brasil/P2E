﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using P2E.Shared.Model;
using P2E.SSO.API.DTO;
using P2E.SSO.API.Helpers;
using P2E.SSO.API.ViewModel;
using P2E.SSO.Domain.Entities;
using P2E.SSO.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Web.Http;

namespace P2E.SSO.API.Controllers
{
    [Authorize]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioModuloRepository _usuarioModuloRepository;
        private readonly IUsuarioGrupoRepository _usuarioGrupoRepository;
        private readonly IGrupoRepository _grupoRepository;
        private readonly IModuloRepository _moduloRepository;
        private readonly IRotinaRepository _rotinaRepository;
        private readonly IRotinaGrupoOperacaoRepository _rotinaGrupoOperacaoRepository;
        private readonly IRotinaUsuarioOperacaoRepository _rotinaUsuarioOperacaoRepository;
        private readonly IOperacaoRepository _operacaoRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IRotinaAssociadaRepository _rotinaAssociadaRepository;


        private readonly AppSettings _appSettings;

        private readonly IMapper _mapper;
        public UsuarioController(IUsuarioRepository usuarioRepository,
                                 IUsuarioModuloRepository usuarioModuloRepository,
                                 IUsuarioGrupoRepository usuarioGrupoRepository,
                                 IModuloRepository moduloRepository,
                                 IGrupoRepository grupoRepository,
                                 IRotinaRepository rotinaRepository,
                                 IRotinaGrupoOperacaoRepository rotinaGrupoOperacaoRepository,
                                 IRotinaUsuarioOperacaoRepository rotinaUsuarioOperacaoRepository,
                                 IOperacaoRepository operacaoRepository,
                                 IRotinaAssociadaRepository rotinaAssociadaRepository,
                                 IServicoRepository servicoRepository,
                                 IOptions<AppSettings> appSettings,
                                 IMapper mapper)
        {
            _mapper = mapper;
            _appSettings = appSettings.Value;
            _usuarioRepository = usuarioRepository;
            _usuarioModuloRepository = usuarioModuloRepository;
            _usuarioGrupoRepository = usuarioGrupoRepository;
            _grupoRepository = grupoRepository;
            _moduloRepository = moduloRepository;
            _rotinaRepository = rotinaRepository;
            _rotinaGrupoOperacaoRepository = rotinaGrupoOperacaoRepository;
            _rotinaUsuarioOperacaoRepository = rotinaUsuarioOperacaoRepository;
            _operacaoRepository = operacaoRepository;
            _rotinaAssociadaRepository = rotinaAssociadaRepository;
            _servicoRepository = servicoRepository;
        }

        [HttpGet]
        [Route("api/v1/usuario/")]
        public DataPage<Usuario> Get([FromQuery] string tx_nome, [FromQuery] DataPage<Usuario> page)
        {
            page = _usuarioRepository.GetByPage(page, tx_nome);
            return page;
        }

        //[HttpGet]
        //[Route("api/v1/usuario/")]
        //public List<Usuario> Get([FromQuery] string tx_nome, [FromQuery] DataPage<Usuario> page)
        //{
        //    page = _usuarioRepository.GetByPage(page, tx_nome);
        //    return page.Items.ToList();
        //}

        [HttpGet]
        [AllowAnonymous]
        [Route("api/v1/usuario/permissoesgrupo/{id}")]
        public List<UsuarioGrupo> GetPermissoesGrupo(int id)
        {
            return ObterPermissoeGrupo(id);
        }

        private List<UsuarioGrupo> ObterPermissoeGrupo(int id)
        {
            // Obtem os grupos em que o usuario está associado
            var usuarioGrupos = _usuarioGrupoRepository.FindAll(o => o.CD_USR == id).ToList();

            foreach (var usuarioGrupo in usuarioGrupos)
            {
                // Obtem rotinas que estão associadas ao grupo
                var rotinaGrupos = _rotinaGrupoOperacaoRepository.FindAll(p => p.CD_GRP == usuarioGrupo.CD_GRP);

                foreach (var rotinaGrupo in rotinaGrupos)
                {
                    // Carrega a rotina
                    rotinaGrupo.Rotina = _rotinaRepository.Find(p => p.CD_ROT == rotinaGrupo.CD_ROT);

                    // Carrega as Permissões
                    rotinaGrupo.Rotina.Operacoes = _operacaoRepository.FindAll(p => p.CD_OPR == rotinaGrupo.CD_OPR).ToList();
                    rotinaGrupo.Operacao = _operacaoRepository.Find(p => p.CD_OPR == rotinaGrupo.CD_OPR);



                    // Carrega os Serviços
                    rotinaGrupo.Rotina.Servico = _servicoRepository.Find(p => p.CD_SRV == rotinaGrupo.Rotina.CD_SRV);
                }

                usuarioGrupo.ListaRotinaGrupoOperacao.AddRange(rotinaGrupos);
            }

            return usuarioGrupos;
        }

        [HttpGet]
        [Route("api/v1/usuario/permissoesusuario/{id}")]
        [AllowAnonymous]
        public List<RotinaUsuarioOperacao> GetPermissoesUsuario(int id)
        {
            return ObterPermissoesUsuario(id);
        }

        private List<RotinaUsuarioOperacao> ObterPermissoesUsuario(int id)
        {
            // Obtem os grupos em que o usuario está associado
            var rotinaUsuarioOperacoes = _rotinaUsuarioOperacaoRepository.FindAll(o => o.CD_USR == id).ToList();

            foreach (var rotinaUsuario in rotinaUsuarioOperacoes)
            {
                // Carrega a rotina
                rotinaUsuario.Rotina = _rotinaRepository.Find(p => p.CD_ROT == rotinaUsuario.CD_ROT);

                // Carrega as Permissões
                rotinaUsuario.Rotina.Operacoes = _operacaoRepository.FindAll(p => p.CD_OPR == rotinaUsuario.CD_OPR).ToList();
                rotinaUsuario.Operacao = _operacaoRepository.Find(p => p.CD_OPR == rotinaUsuario.CD_OPR);


                // Carrega os Serviços
                rotinaUsuario.Rotina.Servico = _servicoRepository.Find(p => p.CD_SRV == rotinaUsuario.Rotina.CD_SRV);
            }

            return rotinaUsuarioOperacoes;
        }

        [HttpGet]
        [Route("api/v1/usuario/obter-permissoes-menu/{id}")]
        [AllowAnonymous]
        public List<Menu> ObterPermissoesMenu(int id)
        {
            #region Carrega permissões de Usuario x Grupo
            var usuarioGrupos = ObterPermissoeGrupo(id);

            var servicosViewModel = new List<ServicoViewModel>();

            // carregar os serviços
            foreach (var item in usuarioGrupos)
            {
                foreach (var subitem in item.ListaRotinaGrupoOperacao)
                {
                    if (!servicosViewModel.Any(p => p.CD_SRV == subitem.Rotina.Servico.CD_SRV))
                    {
                        var servico = subitem.Rotina.Servico;
                        servicosViewModel.Add(new ServicoViewModel()
                        {
                            CD_SRV = servico.CD_SRV,
                            TXT_DEC = servico.TXT_DEC
                        });
                    }
                }
            }

            // carregar as rotinas dos serviços
            foreach (var item in usuarioGrupos)
            {
                foreach (var subitem in item.ListaRotinaGrupoOperacao)
                {
                    var servico = servicosViewModel.First(p => p.CD_SRV == subitem.Rotina.CD_SRV);

                    if (servico.RotinasViewModel == null)
                        servico.RotinasViewModel = new List<RotinaViewModel>();

                    if (!servico.RotinasViewModel.Any(p => p.CD_ROT == subitem.CD_ROT))
                    {
                        var rotinaViewModel = new RotinaViewModel()
                        {
                            CD_ROT = subitem.Rotina.CD_ROT,
                            TX_NOME = subitem.Rotina.TX_NOME,
                            TX_URL = subitem.Rotina.TX_URL
                        };

                        var lista = ObterRotinasAssociadas(subitem.CD_ROT);
                        rotinaViewModel.RotinasAssociadas = lista;

                        if (rotinaViewModel.OperacoesViewModel == null)
                        {
                            rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                        }

                        if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == subitem.CD_OPR))
                        {
                            rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                            {
                                CD_OPR = subitem.CD_OPR,
                                TX_DSC = subitem.Operacao.TX_DSC
                            });
                        }

                        servico.RotinasViewModel.Add(rotinaViewModel);
                    }
                    else
                    {
                        var rotinaViewModel = servico.RotinasViewModel.FirstOrDefault(p => p.CD_ROT == subitem.CD_ROT);

                        if (rotinaViewModel.OperacoesViewModel == null)
                        {
                            rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                        }

                        if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == subitem.CD_OPR))
                        {
                            rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                            {
                                CD_OPR = subitem.CD_OPR,
                                TX_DSC = subitem.Operacao.TX_DSC
                            });
                        }

                    }
                }
            }
            #endregion

            #region Carrega permissões de Usuario x Rotina
            var usuarioRotinas = ObterPermissoesUsuario(id);

            // carregar os serviços
            foreach (var item in usuarioRotinas)
            {
                if (!servicosViewModel.Any(p => p.CD_SRV == item.Rotina.Servico.CD_SRV))
                {
                    var servico = item.Rotina.Servico;

                    if (!servicosViewModel.Any(p => p.CD_SRV == servico.CD_SRV))
                    {
                        servicosViewModel.Add(new ServicoViewModel()
                        {
                            CD_SRV = servico.CD_SRV,
                            TXT_DEC = servico.TXT_DEC
                        });
                    }
                }
            }

            // carregar as rotinas dos serviços
            foreach (var item in usuarioRotinas)
            {
                var servico = servicosViewModel.First(p => p.CD_SRV == item.Rotina.CD_SRV);

                if (servico.RotinasViewModel == null)
                    servico.RotinasViewModel = new List<RotinaViewModel>();

                if (!servico.RotinasViewModel.Any(p => p.CD_ROT == item.CD_ROT))
                {
                    var rotinaViewModel = new RotinaViewModel()
                    {
                        CD_ROT = item.Rotina.CD_ROT,
                        TX_NOME = item.Rotina.TX_NOME,
                        TX_URL = item.Rotina.TX_URL
                    };

                    if (rotinaViewModel.OperacoesViewModel == null)
                    {
                        rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                    }

                    if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == item.CD_OPR))
                    {
                        rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                        {
                            CD_OPR = item.CD_OPR,
                            TX_DSC = item.Operacao.TX_DSC
                        });
                    }

                    servico.RotinasViewModel.Add(rotinaViewModel);
                }
                else
                {
                    var rotinaViewModel = servico.RotinasViewModel.FirstOrDefault(p => p.CD_ROT == item.CD_ROT);

                    if (rotinaViewModel.OperacoesViewModel == null)
                    {
                        rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                    }

                    if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == item.CD_OPR))
                    {
                        rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                        {
                            CD_OPR = item.CD_OPR,
                            TX_DSC = item.Operacao.TX_DSC
                        });
                    }

                }
            }
            #endregion

            #region Montar Menu
            var listItems = new List<Menu>();
            foreach (var servico in servicosViewModel.Where(p => p.RotinasViewModel.Any(x => x.OperacoesViewModel.Any(q => q.TX_DSC.Contains("Consultar")))))
            {

                var item = new Menu() { title = servico.TXT_DEC, root = true, bullet = "Dot", icon = "flaticon2-architecture-and-city" };

                item.submenu = new List<SubMenu>();

                foreach (var rotina in servico.RotinasViewModel.Where(p => p.OperacoesViewModel.Any(x => x.TX_DSC.Contains("Consultar"))))
                {
                    var listItem = new SubMenu()
                    {
                        title = rotina.TX_NOME,
                        page = rotina.TX_URL,
                        CD_ROT = rotina.CD_ROT
                    };

                    //if (rotina.RotinasAssociadas != null && rotina.RotinasAssociadas.Any())
                    //{
                    //    if (listItem.Associados == null)
                    //    {
                    //        listItem.Associados = new List<ItemAssociado>();
                    //    }

                    //    foreach (var rotinaAssociada in rotina.RotinasAssociadas)
                    //    {

                    //        listItem.Associados.Add(
                    //            new ItemAssociado()
                    //            {
                    //                Title = rotinaAssociada?.Rotina?.TX_NOME,
                    //                Href = rotinaAssociada?.Rotina?.TX_URL
                    //            });
                    //    }

                    //    listItem.jsonAssociados = Newtonsoft.Json.JsonConvert.SerializeObject(listItem.Associados);
                    //}

                    item.submenu.Add(listItem);

                }

                listItems.Add(item);
            }

            //var menu = FillProperties(listItems, seedOnly);
            return listItems; //new SmartNavigation(menu);
            #endregion

        }

        [HttpGet]
        [Route("api/v1/usuario/obter-permissoes/{id}/{nomeRotina}")]
        [AllowAnonymous]
        public List<Permissao> ObterPermissoes(int id, string nomeRotina)
        {
            #region Carrega permissões de Usuario x Grupo
            var usuarioGrupos = ObterPermissoeGrupo(id);

            var servicosViewModel = new List<ServicoViewModel>();

            // carregar os serviços
            foreach (var item in usuarioGrupos)
            {
                foreach (var subitem in item.ListaRotinaGrupoOperacao)
                {
                    if (!servicosViewModel.Any(p => p.CD_SRV == subitem.Rotina.Servico.CD_SRV))
                    {
                        var servico = subitem.Rotina.Servico;
                        servicosViewModel.Add(new ServicoViewModel()
                        {
                            CD_SRV = servico.CD_SRV,
                            TXT_DEC = servico.TXT_DEC
                        });
                    }
                }
            }

            // carregar as rotinas dos serviços
            foreach (var item in usuarioGrupos)
            {
                foreach (var subitem in item.ListaRotinaGrupoOperacao)
                {
                    var servico = servicosViewModel.First(p => p.CD_SRV == subitem.Rotina.CD_SRV);

                    if (servico.RotinasViewModel == null)
                        servico.RotinasViewModel = new List<RotinaViewModel>();

                    if (!servico.RotinasViewModel.Any(p => p.CD_ROT == subitem.CD_ROT))
                    {
                        var rotinaViewModel = new RotinaViewModel()
                        {
                            CD_ROT = subitem.Rotina.CD_ROT,
                            TX_NOME = subitem.Rotina.TX_NOME,
                            TX_URL = subitem.Rotina.TX_URL
                        };

                        var lista = ObterRotinasAssociadas(subitem.CD_ROT);
                        rotinaViewModel.RotinasAssociadas = lista;

                        if (rotinaViewModel.OperacoesViewModel == null)
                        {
                            rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                        }

                        if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == subitem.CD_OPR))
                        {
                            rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                            {
                                CD_OPR = subitem.CD_OPR,
                                TX_DSC = subitem.Operacao.TX_DSC
                            });
                        }

                        servico.RotinasViewModel.Add(rotinaViewModel);
                    }
                    else
                    {
                        var rotinaViewModel = servico.RotinasViewModel.FirstOrDefault(p => p.CD_ROT == subitem.CD_ROT);

                        if (rotinaViewModel.OperacoesViewModel == null)
                        {
                            rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                        }

                        if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == subitem.CD_OPR))
                        {
                            rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                            {
                                CD_OPR = subitem.CD_OPR,
                                TX_DSC = subitem.Operacao.TX_DSC
                            });
                        }

                    }
                }
            }
            #endregion

            #region Carrega permissões de Usuario x Rotina
            var usuarioRotinas = ObterPermissoesUsuario(id);

            // carregar os serviços
            foreach (var item in usuarioRotinas)
            {
                if (!servicosViewModel.Any(p => p.CD_SRV == item.Rotina.Servico.CD_SRV))
                {
                    var servico = item.Rotina.Servico;

                    if (!servicosViewModel.Any(p => p.CD_SRV == servico.CD_SRV))
                    {
                        servicosViewModel.Add(new ServicoViewModel()
                        {
                            CD_SRV = servico.CD_SRV,
                            TXT_DEC = servico.TXT_DEC
                        });
                    }
                }
            }

            // carregar as rotinas dos serviços
            foreach (var item in usuarioRotinas)
            {
                var servico = servicosViewModel.First(p => p.CD_SRV == item.Rotina.CD_SRV);

                if (servico.RotinasViewModel == null)
                    servico.RotinasViewModel = new List<RotinaViewModel>();

                if (!servico.RotinasViewModel.Any(p => p.CD_ROT == item.CD_ROT))
                {
                    var rotinaViewModel = new RotinaViewModel()
                    {
                        CD_ROT = item.Rotina.CD_ROT,
                        TX_NOME = item.Rotina.TX_NOME,
                        TX_URL = item.Rotina.TX_URL
                    };

                    if (rotinaViewModel.OperacoesViewModel == null)
                    {
                        rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                    }

                    if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == item.CD_OPR))
                    {
                        rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                        {
                            CD_OPR = item.CD_OPR,
                            TX_DSC = item.Operacao.TX_DSC
                        });
                    }

                    servico.RotinasViewModel.Add(rotinaViewModel);
                }
                else
                {
                    var rotinaViewModel = servico.RotinasViewModel.FirstOrDefault(p => p.CD_ROT == item.CD_ROT);

                    if (rotinaViewModel.OperacoesViewModel == null)
                    {
                        rotinaViewModel.OperacoesViewModel = new List<OperacaoViewModel>();
                    }

                    if (!rotinaViewModel.OperacoesViewModel.Any(p => p.CD_OPR == item.CD_OPR))
                    {
                        rotinaViewModel.OperacoesViewModel.Add(new OperacaoViewModel()
                        {
                            CD_OPR = item.CD_OPR,
                            TX_DSC = item.Operacao.TX_DSC
                        });
                    }

                }
            }
            #endregion

            #region Montar Menu
            var permissoes = new List<Permissao>();
            foreach (var servico in servicosViewModel)
            {
                foreach (var rotina in servico.RotinasViewModel.Where(p => p.TX_NOME.ToLower() == nomeRotina.ToLower()))
                {
                    foreach (var permissao in rotina.OperacoesViewModel)
                    {
                        permissoes.Add(new Permissao() { TX_DSC = permissao.TX_DSC });
                    }
                }
            }

            return permissoes;
            #endregion

        }

        private List<RotinaAssociada> ObterRotinasAssociadas(int id)
        {
            var retorno = new List<RotinaAssociada>();

            retorno = _rotinaAssociadaRepository.FindAll(p => p.CD_ROT_PRINCIPAL == id).ToList();

            foreach (var item in retorno)
            {
                var rotina = _rotinaRepository.Find(p => p.CD_ROT == item.CD_ROT_ASS);
                item.Rotina = rotina;
                item.NomeRotinaAssociada = rotina.TX_NOME;
            }

            return retorno;
        }


        // GET: api/usuario/5
        [HttpGet]
        [Route("api/v1/usuario/{id}")]
        public Usuario Get(int id)
        {
            Usuario result = new Usuario();

            if (id > 0)
            {
                result = _usuarioRepository.Find(p => p.CD_USR == id);
                result.UsuarioModulo = _usuarioModuloRepository.FindAll(o => o.CD_USR == id).ToList();
                result.UsuarioGrupo = _usuarioGrupoRepository.FindAll(o => o.CD_USR == id).ToList();
                result.RotinaUsuarioOperacao = _rotinaUsuarioOperacaoRepository.FindAll(o => o.CD_USR == id).ToList();
                result.Grupo = _grupoRepository.FindAll().ToList();
                result.Modulo = _moduloRepository.FindAll().ToList();

            }
            else
            {
                result.Grupo = _grupoRepository.FindAll().ToList();
                result.Modulo = _moduloRepository.FindAll().ToList();
            }


            return result;
        }

        // POST: api/usuario
        [HttpPost]
        [Route("api/v1/usuario")]
        public object Post([FromBody] Usuario usuario)
        {
            try
            {
                if (usuario.IsValid() && _usuarioRepository.ValidarDuplicidades(usuario))
                {
                    _usuarioRepository.Insert(usuario);
                    return new { message = "OK" };
                }
                else
                {
                    return new { message = usuario.Notifications.FirstOrDefault().Message };
                }

            }
            catch (Exception ex)
            {
                return new { message = "Error." + ex.Message };
            }
        }

        // POST: api/usuario/login
        [AllowAnonymous]
        [HttpPost]
        [Route("api/v1/usuario/login")]
        public Usuario PostLogin([FromBody] Usuario usuario)
        {
            var usuarioBanco = _usuarioRepository.Find(o => o.TX_LOGIN == usuario.TX_LOGIN && o.TX_SENHA == usuario.TX_SENHA);

            if (usuarioBanco != null)
            {
                if (usuarioBanco.OP_STATUS == Shared.Enum.eStatusUsuario.INATIVO)
                {
                    throw new HttpResponseException() { Status = (int)HttpStatusCode.NotFound, Value = "Usuário inativo" };
                }
                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("r5u8x/A?D(G+KbPe");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, usuario.CD_USR.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                usuarioBanco.API_TOKEN = tokenHandler.WriteToken(token);

            }
            else
            {
                throw new HttpResponseException() { Status = (int)HttpStatusCode.NotFound, Value = "Credenciais não encontradas" };
            }

            return usuarioBanco;
        }

        // PUT: api/usuario/5
        [HttpPut]
        [Route("api/v1/usuario/{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Put(int id, [FromBody] Usuario usuario)
        {
            try
            {
                if (usuario.IsValid() && _usuarioRepository.ValidarDuplicidades(usuario))
                {
                    _usuarioModuloRepository.Delete(o => o.CD_USR == usuario.CD_USR);
                    _usuarioGrupoRepository.Delete(o => o.CD_USR == usuario.CD_USR);
                    _rotinaUsuarioOperacaoRepository.Delete(o => o.CD_USR == usuario.CD_USR);

                    if (id > 0)
                        _usuarioRepository.Update(usuario);
                    else
                        _usuarioRepository.Insert(usuario);

                    foreach (var usuarioModulo in usuario.UsuarioModulo)//lista de associações
                    {
                        usuarioModulo.CD_USR = usuario.CD_USR;
                        _usuarioModuloRepository.Insert(usuarioModulo);
                    }

                    foreach (var usuarioGrupo in usuario.UsuarioGrupo)
                    {
                        usuarioGrupo.CD_USR = usuario.CD_USR;
                        _usuarioGrupoRepository.Insert(usuarioGrupo);
                    }

                    foreach (var rotinaUsuarioOperacao in usuario.RotinaUsuarioOperacao)
                    {
                        rotinaUsuarioOperacao.CD_USR = usuario.CD_USR;

                        _rotinaUsuarioOperacaoRepository.Insert(rotinaUsuarioOperacao);
                    }

                    return Ok(usuario);
                }
                else
                {
                    return BadRequest(usuario.Messages);
                }
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, ex.Message);
            }
        }

        // DELETE: api/usuario/5
        [HttpDelete]
        [Route("api/v1/usuario/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                var objeto = _usuarioRepository.FindById(id);

                _usuarioModuloRepository.ExcluirUsuarioModulos(id);
                _usuarioGrupoRepository.ExcluirUsuarioGrupo(id);
                _rotinaUsuarioOperacaoRepository.Delete(o => o.CD_USR == id);
                _usuarioRepository.Delete(objeto);
                return Ok();

                //var rotinaGrupos = _rotinaUsuarioOperacaoRepository.Find(p => p.CD_USR == id);

                //if (rotinaGrupos != null)
                //    return BadRequest("Não foi possivel excluir esse usuario pois ele já tem associações.");
                //else
                //{
                //    _usuarioModuloRepository.ExcluirUsuarioModulos(id);
                //    _usuarioGrupoRepository.ExcluirUsuarioGrupo(id);
                //    _rotinaUsuarioOperacaoRepository.Delete(o => o.CD_USR == id);
                //    _usuarioRepository.Delete(objeto);
                //    return Ok();
                //}
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao tentar excluir o registro. {ex.Message}");
            }
        }

        [HttpGet]
        [Route("api/v1/usuario/valida/{login}/{id}")]
        public bool VerificiarLoginExistente(string login, int id)
        {
            Usuario usuarioOk = new Usuario();
            bool resultado = false;

            usuarioOk = _usuarioRepository.Find(p => p.TX_LOGIN == login);
            if (usuarioOk == null || (usuarioOk != null && id == usuarioOk.CD_USR))
            {
                resultado = true;
            }
            else
            {
                throw new HttpResponseException() { Status = (int)HttpStatusCode.NotFound, Value = "Login já existente" };

            }

            return resultado;
        }
    }
}