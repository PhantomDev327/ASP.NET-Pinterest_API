using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IBaseRepository<Entity> where Entity : class
    {
        Task<Entity> GetByIdAsync(int entityId, string column);
        Task InsertAsync(Entity entity);
        Task AddRangeAsync(IEnumerable<Entity> entities);
        void AttachRange(IEnumerable<Entity> entities);
        void Remove(Entity entity, string property);
        void Update(Entity entity);
        Task<IEnumerable<Entity>> GetAllAsync();
        Task<IEnumerable<Entity>> FindAsync(Expression<Func<Entity, bool>> expression);
    }
}
