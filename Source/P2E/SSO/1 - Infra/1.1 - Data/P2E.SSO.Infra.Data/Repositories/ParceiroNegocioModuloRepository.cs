﻿using MicroOrm.Dapper.Repositories;
using P2E.SSO.Domain.Entities;
using P2E.SSO.Domain.Repositories;
using P2E.SSO.Infra.Data.DataContext;

namespace P2E.SSO.Infra.Data.Repositories
{
    public class ParceiroNegocioModuloRepository : DapperRepository<ParceiroNegocioServicoModulo>, IParceiroNegocioModuloRepository
    {
        private readonly SSOContext _context;

        public ParceiroNegocioModuloRepository(SSOContext context) : base(context.Connection)
        {
            _context = context;
        }
    }
}
