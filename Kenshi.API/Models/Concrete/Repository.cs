using System.Linq.Expressions;
using Kenshi.API.Models.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Kenshi.API.Models.Concrete;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public TEntity GetById(int id)
    {
        return _dbSet.Find(id);
    }
    
    public IQueryable<TEntity> WithQuery()
    {
        return _dbSet.AsQueryable();
    }
    
    public IEnumerable<TEntity> GetAll()
    {
        return _dbSet.ToList();
    }

    public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate);
    }

    public void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    public void AddRange(IEnumerable<TEntity> entities)
    {
        _dbSet.AddRange(entities);
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }
    
    public void Update(TEntity entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
    }
    
    public void Persist(TEntity entity)
    {
        using (var transaction = _dbContext.Database.BeginTransaction())
        {
            if (entity.Id == 0)
            {
                // Enable IDENTITY_INSERT for new entities
                //_dbContext.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {entity.GetType().Name} OFF");

                _dbSet.Add(entity);
            }
            else
            {
                // Check if the entity exists in the database
                var existingEntity = _dbSet.Find(entity.Id);

                if (existingEntity != null)
                {
                    // Detach the existing entity to avoid conflicts
                    _dbSet.Entry(existingEntity).State = EntityState.Detached;

                    // Update the entity with the new Id
                    entity.Id = existingEntity.Id;

                    // Disable IDENTITY_INSERT for existing entities
                    //_dbContext.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {entity.GetType().Name} ON");

                    _dbSet.Update(entity);
                }
                else
                {
                    // Entity with the specified Id does not exist
                    throw new InvalidOperationException("Entity with the specified Id does not exist.");
                }
                
                _dbSet.Update(entity);
            }

            _dbContext.SaveChanges();

            transaction.Commit();
        }
    }

    private void ExecuteSqlCommand(string sql)
    {
        _dbContext.Database.ExecuteSqlRaw(sql);
    }
}