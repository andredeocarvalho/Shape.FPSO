using Shape.FPSO.Models;

namespace Shape.FPSO.Tests.Mocks
{
    public class MockShapeRepository
    {
        private readonly List<Vessel> _vessels = new List<Vessel>();
        private readonly List<Equipment> _equipments = new List<Equipment>();

        public IQueryable<Vessel> Vessels => _vessels.AsQueryable();
        public IQueryable<Equipment> Equipments => _equipments.AsQueryable();

    }
}
