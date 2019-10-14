﻿using P2E.Shared.Model;

namespace P2E.Main.UI.Web.Models.IMP.DetalheNCM
{
    public class DetalheNCMListViewModel
    {
        public DetalheNCMListViewModel()
        {
            DataPage = new DataPage<P2E.Importacao.Domain.Entities.DetalheNCM>();
        }

        public string codigo { get; set; }
        public string detalhe { get; set; }
        public string descricao { get; set; }

        public DataPage<P2E.Importacao.Domain.Entities.DetalheNCM> DataPage { get; set; }
    }
}
