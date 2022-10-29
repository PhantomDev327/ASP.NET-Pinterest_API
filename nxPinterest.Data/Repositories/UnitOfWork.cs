using nxPinterest.Data.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        #region Property
        private readonly ApplicationDbContext _context;
        private bool _disposed;
        #endregion

        #region Constructor
        public UnitOfWork(ApplicationDbContext context) => this._context = context;
        #endregion

        #region Method
        public async Task CompleteAsync() =>
            await _context.SaveChangesAsync();

        protected virtual void Clean(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Clean(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
