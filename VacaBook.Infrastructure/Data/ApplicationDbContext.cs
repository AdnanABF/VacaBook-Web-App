using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacaBook.Domain.Entities;

namespace VacaBook.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Villa> Villas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Villa>().HasData(new Villa
            {
                Id = 1,
                Name = "Premium Pool Villa",
                Description = "This is the description for the premium pool villa",
                ImageUrl = "https://placehold.co/600x401",
                Occupancy = 4,
                Price = 1000,
                Sqft = 1000
            });
        }
    }
}
