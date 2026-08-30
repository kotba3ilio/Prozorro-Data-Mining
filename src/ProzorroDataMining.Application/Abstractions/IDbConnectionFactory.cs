using System.Data;

namespace ProzorroDataMining.Application.Abstractions;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
