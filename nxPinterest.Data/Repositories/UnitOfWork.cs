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
        public int SaveChanges()
        {
            // If any error throw, handle exception by your own way
            var record = _context.SaveChanges();
            return record;
        }

        /// <inheritdoc />
        /// <summary>
        /// Save any change in database context, with async task
        /// </summary>
        /// <returns>
        /// Total effected records
        /// </returns>
        public async Task<int> SaveChangesAsync()
        {
            var record = await _context.SaveChangesAsync();
            return record;
        }

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
