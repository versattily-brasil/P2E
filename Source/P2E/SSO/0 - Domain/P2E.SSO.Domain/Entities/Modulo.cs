﻿using FluentValidator;
using MicroOrm.Dapper.Repositories.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using P2E.Shared.Message;

namespace P2E.SSO.Domain.Entities
{
    [Table("TB_MOD")]
    public class Modulo : CustomNotifiable
    {
        public Modulo()
        {

        }

        public Modulo(string tx_dsc)
        {
            TX_DSC = tx_dsc;

            IsValid();
        }
        
        [Key]
        [Identity]
        public int CD_MOD { get; set; }
        public string TX_DSC { get; set; }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(TX_DSC.Trim()))
                AddNotification("TX_DSC", $"Descrição é um campo obrigatório.");

            return Valid;
        }

        public override string ToString() => $"{TX_DSC.ToString()}";
    }
}
