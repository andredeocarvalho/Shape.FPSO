using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shape.FPSO.Data;
using Shape.FPSO.Models;
using System.Data;
using System.Text.Json.Nodes;

namespace Shape.FPSO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VesselController : ControllerBase
    {
        private readonly FpsoContext _context;

        public VesselController(FpsoContext context)
        {
            _context = context;
        }

        #region Vessel
        [HttpGet]
        public async Task<ActionResult<Vessel[]>> GetVessel()
        {
            return Ok(_context.Vessels.ToArray());
        }

        [HttpGet("{code}")]
        public async Task<ActionResult<Vessel>> GetVessel(string code)
        {
            var vessel = await _context.Vessels.FirstOrDefaultAsync(v => v.Code == code);
            if (vessel == null)
                return NotFound($"The vessel Code informed was not found. Code: {code}");
            return Ok(vessel);
        }

        [HttpPost]
        public async Task<ActionResult<Vessel>> CreateVessel(Vessel vessel)
        {
            try
            {
                ValidateNewVessel(vessel);
                _context.Vessels.Add(vessel);
                await _context.SaveChangesAsync();
                return Ok(await _context.Vessels.FindAsync(vessel.Id));
            }
            catch (DuplicateNameException e)
            {
                return Conflict(e.Message);
            }
        }
        #endregion

        #region Equipment
        [HttpGet("equipment")]
        public async Task<ActionResult<Equipment[]>> GetEquipment() =>
            Ok(await _context.Equipment.ToArrayAsync());

        [HttpGet("equipment/{code}")]
        public async Task<ActionResult<Equipment>> GetEquipment(string code)
        {
            var equipment = await _context.Equipment.FirstOrDefaultAsync(v => v.Code == code);
            if (equipment == null)
                return NotFound($"The equipment Code informed was not found. Code: {code}");
            return Ok(equipment);
        }

        [HttpGet("equipment/formatted")]
        public async Task<ActionResult<dynamic[]>> GetEquipmentFormatted() =>
            Ok(await _context.Equipment.Select(e => new { e.Name, VesselCode = _context.Vessels.First(v => v.Id == e.VesselId).Code }).ToArrayAsync());

        [HttpGet("{code}/equipment")]
        public async Task<ActionResult<Equipment[]>> GetEquipmentByVessel(string code)
        {
            var vesselId = (await _context.Vessels.FirstOrDefaultAsync(v => v.Code == code))?.Id;
            if (vesselId == null)
                return NotFound($"The vessel code informed was not found. Code: {code}");
            return Ok(await _context.Equipment.Where(e => e.VesselId == vesselId.Value).ToArrayAsync());
        }

        [HttpPost("{code}/equipment")]
        public async Task<ActionResult<Equipment>> CreateEquipment(string code, Equipment equipment)
        {
            try
            {
                ValidateNewEquipment(equipment);

                var vessel = _context.Vessels.Where(v => v.Code == code).FirstOrDefault();
                if (vessel == null)
                    return NotFound($"The vessel code informed was not found. Code: {code}");
                equipment.VesselId = vessel.Id;


                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync();
                return Ok(await _context.Equipment.FindAsync(equipment.Id));
            }
            catch (DuplicateNameException e)
            {

                return Conflict(e.Message);
            }
        }

        [HttpGet("{code}/equipment/get_active")]
        public async Task<ActionResult<Equipment[]>> GetActiveEquipment(string code)
        {
            var vessel = await _context.Vessels.FirstOrDefaultAsync(v => v.Code == code);
            if (vessel == null)
                return NotFound($"The vessel code informed was not found. Code: {code}");
            return Ok(await _context.Equipment.Where(e => e.VesselId == vessel.Id && e.Active).ToArrayAsync());
        }

        [HttpPut("equipment/set_inactive")]
        public async Task<ActionResult<Equipment[]>> SetEquipmentToInactive(List<string> codes)
        {
            var activeEquipment = _context.Equipment.Where(e => codes.Contains(e.Code));
            if (activeEquipment?.Any() != true)
                return NotFound($"None of the equipment code informed were found. EquipmentCodes: {string.Join(", ", codes)}");

            var inactiveEquipment = new List<Equipment>();
            foreach (var equipment in activeEquipment)
            {
                codes.Remove(equipment.Code);
                equipment.Active = false;
                _context.Equipment.Update(equipment);
                inactiveEquipment.Add(equipment);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Deactivated = inactiveEquipment, CodesNotFound = codes });
        }
        #endregion

        private void ValidateNewVessel(Vessel vessel)
        {
            if (_context.Vessels.Any(v => v.Code == vessel.Code))
                throw new DuplicateNameException($"A Vessel with the specified Code already exists. Code: {vessel.Code}");
        }

        private void ValidateNewEquipment(Equipment equipment)
        {
            if (_context.Equipment.Any(e => e.Code == equipment.Code))
                throw new DuplicateNameException($"An Equipment with the specified Code already exists. Code: {equipment.Code}");
        }
    }
}
