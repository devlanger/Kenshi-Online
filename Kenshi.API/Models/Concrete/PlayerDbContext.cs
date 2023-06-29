using Kenshi.API.Models.Concrete;
using Microsoft.EntityFrameworkCore;

namespace Kenshi.API.Models;

public class PlayerDbContext : DbContext
{
    public DbSet<PlayerConnection> PlayerConnection { get; set; }
    
    public DbSet<User> User { get; set; }

    public PlayerDbContext(DbContextOptions<PlayerDbContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerConnection>()
            .HasKey(v => v.Id);
        
        modelBuilder.Entity<User>()
            .HasKey(v => v.Id);
    }
}
