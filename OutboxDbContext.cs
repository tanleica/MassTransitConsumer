using Microsoft.EntityFrameworkCore;
namespace MassTransitConsumer;
public class OutboxDbContext : DbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=159.223.59.17,1433;Database=MassTransitDB;User Id=sa;Password=A123231312a@;TrustServerCertificate=True;");
    }
}
