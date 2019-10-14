﻿using MicroOrm.Dapper.Repositories;
using P2E.Administrativo.Domain.Entities;
using P2E.Administrativo.Domain.Repositories;
using P2E.Administrativo.Infra.Data.DataContext;

namespace P2E.Administrativo.Infra.Data.Repositories
{
    public class AgendaExecRepository : DapperRepository<AgendaExec>, IAgendaExecRepository
    {
        private readonly AdmContext _admContext;

        public AgendaExecRepository(AdmContext admContext) : base(admContext.Connection)
        {
            _admContext = admContext;
        }
    }
}
