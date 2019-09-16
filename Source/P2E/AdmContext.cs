using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using P2E.SSO.Domain.Entities.Map;
using DapperExtensions;
using Dapper;
using P2E.Shared.TypeHandler;

namespace P2E.Administrativo.Infra.Data.DataContext
{
    public class AdmContext : IDisposable
    {
        public SqlConnection Connection { get; set; }

        public AdmContext()
        {
            Connection = new SqlConnection(P2E.Shared.Configuration.ConnectionString);

            InicializaMapperDapper();
            var dialect = new DapperExtensions.Sql.SqlServerDialect();
            var conf = new DapperExtensionsConfiguration(null,
                new[]
                {
                    typeof(ParceiroNegocioMap).Assembly
                }, dialect);
            DapperExtensions.DapperExtensions.Configure(conf);
        }

        public void Dispose()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Close();
            }
        }

        public static void InicializaMapperDapper()
        {
            DapperExtensions.DapperExtensions.SetMappingAssemblies(new[]
            {
                typeof(ParceiroNegocioMap).Assembly
            }
            );

            SqlMapper.AddTypeHandler(new DocumentTypeHandler());
        }
    }
}
