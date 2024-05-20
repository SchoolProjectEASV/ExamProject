using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Configuration;
using MongoDB.EntityFrameworkCore.Extensions;

namespace CategoryInfrastructure
{
    /// <summary>
    /// Represents the database context for Category
    /// </summary>
    public class CategoryDbContext : DbContext
    {

        public DbSet<Category> Categories { get; set; }

        private IVaultFactory _vaultFactory;
        private string? _connectionString;
        
        public CategoryDbContext(IVaultFactory vaultFactory)
        {
            _vaultFactory = vaultFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMongoDB(GetConnectionString(), "Category");
        }


        public string GetConnectionString()
        {
            _connectionString = _vaultFactory.GetConnectionStringCategory();
            return _connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().ToCollection("categories");
        }

    }
}
