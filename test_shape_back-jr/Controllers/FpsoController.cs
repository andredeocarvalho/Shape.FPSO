using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shape.FPSO.Data;
using Shape.FPSO.Models;
using System.Data;

namespace Shape.FPSO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FpsoController : ControllerBase
    {
        private readonly FpsoContext _context;

        public FpsoController(FpsoContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Vessel>>> GetVessels() =>
            Ok(await _context.Vessels.ToListAsync());

        [HttpPost]
        public async Task<ActionResult<Vessel>> CreateVessel(Vessel vessel)
        {
            try
            {
                ValidateVessel(vessel);
                _context.Vessels.Add(vessel);
                await _context.SaveChangesAsync();
                return Ok(await _context.Vessels.FindAsync(vessel.Id));
            }
            catch (DuplicateNameException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpGet("Equipment")]
        public async Task<ActionResult<List<dynamic>>> GetEquipments() =>
            Ok(await _context.Equipments.Select(e => new { e.Name, VesselCode = _context.Vessels.FirstOrDefault(v => v.Id == e.VesselId).Code }).ToListAsync());

        [HttpPost("{vesselCode}/Equipment")]
        public async Task<ActionResult<Equipment>> CreateEquipment(string vesselCode, Equipment equipment)
        {
            try
            {
                ValidateEquipment(equipment);

                var vessel = _context.Vessels.Where(v => v.Code == vesselCode).FirstOrDefault();
                if (vessel == null)
                    return NotFound($"The vessel code informed was not found. VesselCode: {vesselCode}");
                equipment.VesselId = vessel.Id;


                _context.Equipments.Add(equipment);
                await _context.SaveChangesAsync();
                return Ok(await _context.Equipments.FindAsync(equipment.Id));
            }
            catch (DuplicateNameException e)
            {

                return Conflict(e.Message);
            }
        }

        [HttpGet("{vesselCode}/Equipment/GetActive")]
        public async Task<ActionResult<List<Equipment>>> GetActiveVessels() =>
            Ok(await _context.Equipments.Where(e => e.Active).ToListAsync());

        [HttpPut("Equipment/SetInactive")]
        public async Task<ActionResult<List<Equipment>>> SetEquipmentToInactive(string[] codes)
        {
            var activeEquipments = _context.Equipments.Where(e => codes.Contains(e.Code));
            if (activeEquipments?.Any() != true)
                return NotFound($"None of the equipments informed were found. EquipmentCodes: {string.Join(", ", codes)}");

            foreach (var equipment in activeEquipments)
            {
                equipment.Active = false;
                _context.Equipments.Update(equipment);
            }
            _context.SaveChanges();
            return Ok();
        }
        private void ValidateVessel(Vessel vessel)
        {
            if (_context.Vessels.Any(v => v.Code == vessel.Code))
                throw new DuplicateNameException($"A Vessel with the specified Code already exists. Code: {vessel.Code}");
        }

        private void ValidateEquipment(Equipment equipment)
        {
            if (_context.Equipments.Any(e => e.Code == equipment.Code))
                throw new DuplicateNameException($"An Equipment with the specified Code already exists. Code: {equipment.Code}");
        }
    }
}
