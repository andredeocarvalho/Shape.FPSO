using Microsoft.EntityFrameworkCore;
using Shape.FPSO.Models;

namespace Shape.FPSO.Data
{
    public class FpsoContext : DbContext
    {
        public FpsoContext(DbContextOptions<FpsoContext> options) : base(options)
        {

        }

        public DbSet<Vessel> Vessels => Set<Vessel>();
        public DbSet<Equipment> Equipments => Set<Equipment>();
    }
}
