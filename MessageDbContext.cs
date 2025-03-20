using Microsoft.EntityFrameworkCore;

public class MessageDbContext : DbContext
{
    public DbSet<MessageLog> Messages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=159.223.59.17;Database=RabbitMQLogs;User Id=sa;Password=A123231312a@;TrustServerCertificate=True;");
    }
}

public class MessageLog
{
    public int Id { get; set; }
    public string Message { get; set; } = null!;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
