using System;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Save any change in database context. Throw exception if any.
        /// </summary>
        /// <returns>Total effected records</returns>
        int SaveChanges();

        /// <summary>
        /// Save any change in database context, with async task
        /// Usage: int saved = await SaveChangesAsync();
        /// </summary>
        /// <returns>Total effected records</returns>
        Task<int> SaveChangesAsync();
    }
}
