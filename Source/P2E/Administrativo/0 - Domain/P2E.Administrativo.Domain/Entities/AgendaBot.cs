﻿using MicroOrm.Dapper.Repositories.Attributes;
using P2E.Shared.Message;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2E.Administrativo.Domain.Entities
{
    [Table("TB_AGENDA_BOT")]
    public class AgendaBot : CustomNotifiable
    {
        [Key]
        [Identity]
        public int CD_AGENDA_BOT { get; set; }
        public int CD_AGENDA { get; set; }
        public int CD_BOT { get; set; }
        public int NR_ORDEM_EXEC { get; set; }
        public string TX_PARAM_EXEC { get; set; }

        public int CD_ULTIMO_STATUS_EXEC_BOT { get; set; }
        public int? CD_ULTIMA_EXEC_BOT { get; set; }

        public Agenda Agenda { get; set; }
        public Bot Bot { get; set; }
    }
}
