using System;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task CompleteAsync();
    }
}
