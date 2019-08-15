﻿using P2E.Shared.Enum;
using System.Collections.Generic;
using P2E.SSO.Domain.Entities;
using P2E.Main.UI.Web.Models.SSO.Servico;
using P2E.Shared.Model;

namespace P2E.Main.UI.Web.Models.SSO.Rotina
{
    public class RotinaListViewModel
    {
        public RotinaListViewModel()
        {
            DataPage = new DataPage<P2E.SSO.Domain.Entities.Rotina>();
        }

        public string Nome { get;  set; }
        public string Descricao { get; set; }
        public string ServicoDesc { get; set; }
        public string Url { get; set; }

        public DataPage<P2E.SSO.Domain.Entities.Rotina> DataPage { get; set; }

        public List<P2E.SSO.Domain.Entities.Servico> Servicos { get; set; }
       // public P2E.SSO.Domain.Entities.Servico Servico { get; set; }
    }
}
