using Microsoft.Extensions.Configuration;
using Npgsql;
using SmartBar.Application.Interfaces;
using System.Data;

namespace SmartBar.Infrastructure
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SmartBarConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
