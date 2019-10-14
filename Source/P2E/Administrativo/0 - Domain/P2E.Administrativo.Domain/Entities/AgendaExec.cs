﻿using MicroOrm.Dapper.Repositories.Attributes;
using P2E.Shared.Enum;
using P2E.Shared.Message;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2E.Administrativo.Domain.Entities
{
    [Table("TB_AGENDA_EXEC")]
    public class AgendaExec : CustomNotifiable
    {
        [Key]
        [Identity]
        public int CD_AGENDA_EXEC { get; set; }
        public int CD_AGENDA { get; set; }
        public DateTime? DT_INICIO_EXEC { get; set; }
        public DateTime? DT_FIM_EXEC { get; set; }
        public eStatusExec OP_STATUS_AGENDA_EXEC { get; set; }
    }
}
