using Microsoft.EntityFrameworkCore;
using WalletApi.Models;

namespace WalletApi.Data
{
   public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId);

        builder.Entity<Account>()
            .Navigation(a => a.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Entity<Account>()
            .Property(a => a.Version)
            .IsRowVersion();
        
        builder.Entity<Transaction>()
            .HasIndex(t => new { t.AccountId, t.CreatedAt });


        builder.Entity<IdempotencyRecord>()
            .HasIndex(x => x.Key)
            .IsUnique();


        base.OnModelCreating(builder);
    }
}
}