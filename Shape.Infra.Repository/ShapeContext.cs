using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Shape.WebApi.Rest.Models;

namespace Shape.Data
{
    public class ShapeContext : DbContext
    {
        public ShapeContext(DbContextOptions<ShapeContext> options) : base(options)
        {

        }

        public DbSet<Vessel> Vessels => Set<Vessel>();
        public DbSet<Equipment> Equipments => Set<Equipment>();
    }
}
