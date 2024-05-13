using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace CategoryInfrastructure
{
    /// <summary>
    /// Represents the database context for Category
    /// </summary>
    public class CategoryDbContext : DbContext
    {

        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMongoDB("placeholder", "Category");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().ToCollection("categories");
        }

    }
}
