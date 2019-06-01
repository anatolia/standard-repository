using System.Data.Common;

namespace StandardRepository.Factories
{
    public interface IConnectionFactory<TConnection> where TConnection : DbConnection
    {
        TConnection Create();
    }
}