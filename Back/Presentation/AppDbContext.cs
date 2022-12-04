using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Presentation.Options;

namespace Presentation;

public class AppDbContext : DbContext
{
    private readonly DbOptions _options;
    public AppDbContext(IOptions<DbOptions> options) => _options = options.Value;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseNpgsql(_options.ConnectionString);

    public DbSet<Message> Messages { get; set; } = null!;
}