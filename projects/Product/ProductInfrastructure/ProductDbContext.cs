using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoClient;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductInfrastructure
{
    public class ProductDbContext : DbContext
    {
        private readonly IMongoDatabase _database;


        public ProductDbContext(Client customClient, string databaseName)
        {
            _database = customClient.GetDatabase(databaseName) ?? throw new ArgumentNullException(nameof(databaseName), "Database could not be found.");
        }

        public DbSet<Product> Products { get; set; }

        public static ProductDbContext Create(Client customClient, string databaseName)
        {
            var dbContextOptions = new DbContextOptionsBuilder<ProductDbContext>()
                .UseMongoDB(customClient.GetDatabase(databaseName).Client, databaseName)
                .Options;

            return new ProductDbContext(dbContextOptions);
        }
        public ProductDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Product>().ToCollection("products");
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}
