﻿using DapperExtensions.Mapper;

namespace P2E.Administrativo.Domain.Entities.Map
{
    public class BotExecMap : ClassMapper<BotExec>
    {
        public BotExecMap()
        {
            Table("TB_BOT_EXEC");

            Map(p => p.CD_BOT_EXEC).Column("CD_BOT_EXEC").Key(KeyType.Identity);
            Map(p => p.CD_BOT).Column("CD_BOT");
            Map(p => p.CD_AGENDA_EXEC).Column("CD_AGENDA_EXEC");
            Map(p => p.NR_ORDEM_EXEC).Column("NR_ORDEM_EXEC");
            Map(p => p.OP_STATUS_BOT_EXEC).Column("OP_STATUS_BOT_EXEC");
            Map(p => p.DT_INICIO_EXEC).Column("DT_INICIO_EXEC");
            Map(p => p.DT_FIM_EXEC).Column("DT_FIM_EXEC");
        }
    }
}
