using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ProzorroDataMining.Application.Abstractions;

namespace ProzorroDataMining.Infrastructure.Database;

public sealed class NpgsqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        return new NpgsqlConnection(connectionString);
    }
}
