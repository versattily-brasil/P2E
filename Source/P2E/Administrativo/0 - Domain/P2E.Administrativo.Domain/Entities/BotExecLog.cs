﻿using MicroOrm.Dapper.Repositories.Attributes;
using P2E.Shared.Enum;
using P2E.Shared.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2E.Administrativo.Domain.Entities
{
    [Table("TB_BOT_EXEC_LOG")]
    public class BotExecLog : CustomNotifiable
    {
        [Key]
        [Identity]
        public int CD_BOT_EXEC_LOG { get; set; }
        public eTipoLog OP_TIPO_LOG { get; set; }
        public string TX_MENSAGEM { get; set; }
        public int CD_BOT_EXEC { get; set; }
    }
}
