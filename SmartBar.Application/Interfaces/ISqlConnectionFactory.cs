using System.Data;

namespace SmartBar.Application.Interfaces;
public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}