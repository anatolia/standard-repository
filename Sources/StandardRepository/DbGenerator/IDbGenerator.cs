using System.Threading.Tasks;

namespace StandardRepository.DbGenerator
{
    public interface IDbGenerator
    {
        bool IsDbExistsDb(string dbName = null);
        void CreateDb(string dbName = null);
        Task Generate();
    }
}