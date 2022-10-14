using Domain;
using Microsoft.EntityFrameworkCore;

namespace Presentation;

public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=database;Port=5432;Username=testuser;Password=testpass;Database=testDb;");
    }

    public DbSet<Message> Messages { get; set; } = null!;
}