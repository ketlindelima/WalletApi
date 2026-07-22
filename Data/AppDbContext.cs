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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId);

        builder.Entity<Account>()
            .Navigation(a => a.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        base.OnModelCreating(builder);
    }
}
}