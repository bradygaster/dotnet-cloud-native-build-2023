using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Orders;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderDatabaseRecord> OrderItems => Set<OrderDatabaseRecord>();
    public DbSet<CartItemDatabaseRecord> CartItems => Set<CartItemDatabaseRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        DefineOrderRecord(builder.Entity<OrderDatabaseRecord>());

        DefineCartItemRecord(builder.Entity<CartItemDatabaseRecord>()); 
    }

    private static void DefineOrderRecord(EntityTypeBuilder<OrderDatabaseRecord> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(ci => ci.OrderId);

        builder.Property(ci => ci.OrderId)
            .IsRequired();

        builder.Property(cb => cb.OrderedAt)
            .IsRequired();

        builder.Property(cc => cc.HasShipped)
            .IsRequired()
            .HasDefaultValue(false);
    }

    private static void DefineCartItemRecord(EntityTypeBuilder<CartItemDatabaseRecord> builder)
    {
        builder.ToTable("cartitems");

        builder.HasKey(ci => ci.CartItemId);

        builder.Property(ci => ci.CartItemId)
            .IsRequired();

        builder.Property(ci => ci.ProductId)
            .IsRequired();

        builder.Property(ci => ci.OrderId)
            .IsRequired();

        builder.Property(ci => ci.Quantity)
            .IsRequired();
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> asyncEnumerable)
    {
        var results = new List<T>();
        await foreach (var value in asyncEnumerable)
        {
            results.Add(value);
        }

        return results;
    }
}
