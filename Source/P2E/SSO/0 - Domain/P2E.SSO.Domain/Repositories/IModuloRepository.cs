﻿using DapperExtensions;
using MicroOrm.Dapper.Repositories;
using P2E.Shared.Model;
using P2E.SSO.Domain.Entities;

namespace P2E.SSO.Domain.Repositories
{
    public interface IModuloRepository : IDapperRepository<Modulo>
    {
        DataPage<Modulo> GetByPage(DataPage<Modulo> page, string tx_dsc);

        int GetTotalRows(PredicateGroup predicateGroup);

        bool ValidarDuplicidades(Modulo modulo);
    }
}
