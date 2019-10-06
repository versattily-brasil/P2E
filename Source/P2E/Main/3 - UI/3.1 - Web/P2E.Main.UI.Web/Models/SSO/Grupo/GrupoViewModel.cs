﻿using P2E.Main.UI.Web.Models.SSO.Operacao;
using P2E.Main.UI.Web.Models.SSO.Rotina;
using P2E.Main.UI.Web.Models.SSO.Servico;
using System.Collections.Generic;

namespace P2E.Main.UI.Web.Models.SSO.Grupo
{
    public class GrupoViewModel
    {
        public int CD_GRP { get; set; }
        public string TX_DSC { get; set; }

        public IList<RotinaViewModel> Rotinas { get; set; } = new List<RotinaViewModel>();
        public List<OperacaoViewModel> Operacoes { get; set; } = new List<OperacaoViewModel>();
        public IList<ServicoViewModel> Servicos { get; set; } = new List<ServicoViewModel>();

        public List<RotinaGrupoViewModel> RotinaGrupoOperacao { get; set; } = new List<RotinaGrupoViewModel>();
    }
}
