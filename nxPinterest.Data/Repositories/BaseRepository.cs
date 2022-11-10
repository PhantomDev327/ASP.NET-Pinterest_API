using Microsoft.EntityFrameworkCore;
using nxPinterest.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        #region Property
        protected readonly ApplicationDbContext Context;
        private DbSet<T> _entities;
        #endregion

        #region Constructor
        public BaseRepository(ApplicationDbContext context)
        {
            this.Context = context;
            this._entities = Context.Set<T>();
        }

        public virtual async Task Add(T entity)
        {
            await _entities.AddAsync(entity);
        }

        public bool CheckContains(Expression<Func<T, bool>> predicate)
        {
            return Context.Set<T>().Count<T>(predicate) > 0;
        }

        public int Count(Expression<Func<T, bool>> where)
        {
            return _entities.Count(where);
        }

        public T Delete(T entity)
        {
            return _entities.Remove(entity).Entity;
        }

        public T Delete(int id)
        {
            var entity = _entities.Find(id);
            return _entities.Remove(entity).Entity;
        }

        public void DeleteMulti(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = _entities.Where<T>(where).AsEnumerable();
            foreach (T obj in objects)
                _entities.Remove(obj);
        }

        public IEnumerable<T> GetAll(string[] includes = null)
        {
            //HANDLE INCLUDES FOR ASSOCIATED OBJECTS IF APPLICABLE
            if (includes != null && includes.Count() > 0)
            {
                var query = Context.Set<T>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                return query.AsQueryable();
            }

            return Context.Set<T>().AsQueryable();
        }


        public T GetSingleByCondition(Expression<Func<T, bool>> expression, string[] includes = null)
        {
            if (includes != null && includes.Count() > 0)
            {
                var query = Context.Set<T>().Include(includes.First());
                foreach (var include in includes.Skip(1))
                    query = query.Include(include);
                return query.FirstOrDefault(expression);
            }
            return Context.Set<T>().FirstOrDefault(expression);
        }

        public T GetSingleById(int id)
        {
            return _entities.Find(id);
        }

        public T GetSingleById(string id)
        {
            return _entities.Find(id);
        }

        public void Update(T entity)
        {
            _entities.Attach(entity);
            Context.Entry(entity).State = EntityState.Modified;
        }
        #endregion

    }
}
