using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacaBook.Application.Common.Interfaces;
using VacaBook.Domain.Entities;
using VacaBook.Infrastructure.Data;

namespace VacaBook.Infrastructure.Repository
{
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AmenityRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(Amenity entity)
        {
            _dbContext.Amenities.Update(entity);
        }
    }
}
