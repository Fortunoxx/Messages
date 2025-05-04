using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Database.Contexts;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure self-referencing relationship for Message
        builder.Entity<Message>()
            .HasOne<Message>() // Parent Message
            .WithMany()        // No navigation property for children
            .HasForeignKey(m => m.MessageIdRef) // Foreign key
            .OnDelete(DeleteBehavior.Restrict); // Optional: Configure delete behavior
    }
}

public interface IApplicationDbContext
{
    DbSet<Attachment> Attachments { get; }
    DbSet<Message> Messages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public record Message
{
    public required ICollection<Attachment> Attachments { get; init; } = [];
    public required string Content { get; init; }
    public int Id { get; init; }
    public bool IsRead { get; set; }
    public int? MessageIdRef { get; init; } // Nullable for self-referencing
    public int Receiver { get; init; }
    public int Sender { get; init; }
    public DateTime SentAt { get; init; } = DateTime.UtcNow;    
    public required string Title { get; init; }
}

public record Attachment(int Id, int MessageId, string FileName, string ContentType, long Size, byte[] Data);