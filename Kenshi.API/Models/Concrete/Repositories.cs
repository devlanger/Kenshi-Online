using Kenshi.API.Models.Abstract;

namespace Kenshi.API.Models.Concrete;

public class MasterRepository<TEntity> : Repository<TEntity> where TEntity : BaseEntity
{
    private readonly MasterDbContext _dbContext;

    public MasterRepository(MasterDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}

public class PlayerRepository<TEntity> : Repository<TEntity> where TEntity : BaseEntity
{
    private readonly PlayerDbContext _dbContext;

    public PlayerRepository(PlayerDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
}