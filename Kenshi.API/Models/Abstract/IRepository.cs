using System.Linq.Expressions;

namespace Kenshi.API.Models.Abstract;

public interface IRepository<TEntity> where TEntity : class
{
    TEntity GetById(int id);
    IQueryable<TEntity> WithQuery();
    IEnumerable<TEntity> GetAll();
    IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
    void RemoveRange(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Persist(TEntity entity);
}