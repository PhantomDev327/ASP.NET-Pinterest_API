using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public abstract class BaseRepository<Entity> : IBaseRepository<Entity> where Entity : class
    {
        #region Property
        protected readonly ApplicationDbContext Context;
        private DbSet<Entity> _entities;
        #endregion

        #region Constructor
        public BaseRepository(ApplicationDbContext context)
        {
            this.Context = context;
            this._entities = Context.Set<Entity>();
        }
        #endregion

        #region Method
        public virtual async Task<IEnumerable<Entity>> FindAsync(Expression<Func<Entity, bool>> expression) =>
            await _entities.Where(expression).ToListAsync();

        public virtual async Task<Entity> GetByIdAsync(int entityId, string column) =>
            await _entities.Where(entity => EF.Property<int>(entity, column).Equals(entityId)).SingleOrDefaultAsync();

        public virtual async Task InsertAsync(Entity entity) =>
            await _entities.AddAsync(entity);

        public virtual async Task AddRangeAsync(IEnumerable<Entity> entities) =>
            await _entities.AddRangeAsync(entities);

        public virtual void AttachRange(IEnumerable<Entity> entities) =>
            _entities.AttachRange(entities);

        /// <summary>
        /// Soft-delete by change value of status true -> false
        /// </summary>
        /// <param name="entity">Entity object</param>
        public virtual void Remove(Entity entity, string property) =>
            entity.GetType().GetProperty(property).SetValue(entity, true);

        public virtual void Update(Entity entity) =>
            _entities.Update(entity);

        public virtual async Task<IEnumerable<Entity>> GetAllAsync() =>
            await _entities.AsNoTracking().ToListAsync();


        #endregion
    }
}
