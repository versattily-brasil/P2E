﻿using P2E.Main.UI.Web.Models.SSO.Modulo;
using P2E.Main.UI.Web.Models.SSO.Grupo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2E.Main.UI.Web.Models.SSO.Usuario
{
    public class UsuarioViewModel
    {
        public int CD_USR { get; set; }
        public string TX_LOGIN { get; set; }
        public string TX_NOME { get; set; }
        public string TX_SENHA { get; set; }
        public int OP_STATUS { get; set; }

        public List<UsuarioModuloViewModel> UsuarioModulos { get; set; } = new List<UsuarioModuloViewModel>();
        public List<UsuarioGrupoViewModel> UsuarioGrupo { get; set; } = new List<UsuarioGrupoViewModel>();

        public List<ModuloViewModel> Modulos { get; set; } = new List<ModuloViewModel>();               
        public List<GrupoViewModel> Grupo { get; set; } = new List<GrupoViewModel>();
    }
}
