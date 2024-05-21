using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace ProductInfrastructure
{
    /// <summary>
    /// Represents the database context for Product
    /// </summary>
    public class ProductDbContext : DbContext
    {

        public DbSet<Product> Products { get; set; }

        private  string? _connectionString;

        IVaultFactory _vaultFactory;

        public ProductDbContext(IVaultFactory vaultFactory)
        {
            _vaultFactory = vaultFactory;
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMongoDB(GetConnectionString(), "Product");
        }

        public string GetConnectionString()
        {
            _connectionString = _vaultFactory.GetConnectionStringProduct();
            return _connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToCollection("products");
        }
    }
}
