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
        public DbSet<VillaNumber> VillaNumbers { get; set; }

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

            modelBuilder.Entity<VillaNumber>().HasData(new VillaNumber
            {
                Villa_Number = 101,
                VillaId = 1
            },
            new VillaNumber
            {
                Villa_Number = 102,
                VillaId = 1
            },
            new VillaNumber
            {
                Villa_Number = 103,
                VillaId = 1
            },
            new VillaNumber
            {
                Villa_Number = 104,
                VillaId = 1
            });
        }
    }
}
