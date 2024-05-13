using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            _connectionString = _vaultFactory.GetConnectionStringAsync().Result;
            return _connectionString;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToCollection("products");
        }
    }
}
