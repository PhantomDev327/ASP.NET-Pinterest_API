using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace nxPinterest.Data.Repositories.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        // Marks an entity as new
        Task Add(T entity);

        // Marks an entity as modified
        void Update(T entity);

        // Marks an entity to be removed
        T Delete(T entity);

        T Delete(int id);

        //Delete multi records
        void DeleteMulti(Expression<Func<T, bool>> where);

        // Get an entity by int id
        T GetSingleById(int id);

        T GetSingleById(string id);

        T GetSingleByCondition(Expression<Func<T, bool>> expression, string[] includes = null);

        IEnumerable<T> GetAll(string[] includes = null);

        int Count(Expression<Func<T, bool>> where);

        bool CheckContains(Expression<Func<T, bool>> predicate);


        // <summary>
        /// Attach then update entity, can specify properties to update, or update all except exclude properties
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="updateProperties">if has value, update these properties instead all properties of entity</param>
        /// <param name="excludeProperties">if has value, exclude these properties out of update process</param>
        /// <returns>The updated entity</returns>
        T Update(T entity, List<Expression<Func<T, object>>> updateProperties = null, List<Expression<Func<T, object>>> excludeProperties = null);
    }
}
