using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Controllers
{
    [Route("api/environmentobjects")]
    [ApiController]
    public class EnvironmentObjectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnvironmentObjectController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ObjectDto>>> GetObjects()
        {
            var objects = await _context.Objects
                .Select(o => new ObjectDto
                {
                    IdObject = o.IdObject,
                    Name = o.Name,
                    Description = o.Description
                })
                .ToListAsync();

            return Ok(objects);
        }
        // GET: api/AplinkosObjektai/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ObjectDto>> GetObject(int id)
        {
            var obj = await _context.Objects.FindAsync(id);
            if (obj == null) return NotFound();

            return Ok(new ObjectDto
            {
                IdObject = obj.IdObject,
                Name = obj.Name,
                Description = obj.Description
            });
        }

        // POST: api/AplinkosObjektai
        [HttpPost]
        public async Task<ActionResult<ObjectDto>> CreateObject(ObjectCreateDto dto)
        {
            var obj = new EnvironmentObject
            {
                Name = dto.Name,
                Description = dto.Description,
                TotalMass = dto.TotalMass,
                TotalVolume = dto.TotalVolume,
                CreatedAt = DateTime.UtcNow
            };

            _context.Objects.Add(obj);
            await _context.SaveChangesAsync();

            var response = new ObjectDto
            {
                IdObject = obj.IdObject,
                Name = obj.Name,
                Description = obj.Description
            };

            return CreatedAtAction(nameof(GetObject), new { id = obj.IdObject }, response);
        }

        // PUT: api/AplinkosObjektai/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateObject(int id, ObjectCreateDto dto)
        {
            var obj = await _context.Objects.FindAsync(id);
            if (obj == null) return NotFound();

            obj.Name = dto.Name;
            obj.Description = dto.Description;
            obj.TotalMass = dto.TotalMass;
            obj.TotalVolume = dto.TotalVolume;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/AplinkosObjektai/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObject(int id)
        {
            var obj = await _context.Objects.FindAsync(id);
            if (obj == null) return NotFound();

            _context.Objects.Remove(obj);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // Jei reikia, pridėkite POST, PUT, DELETE metodus objektų valdymui
        // GET: api/environmentobjects/{id}/materials
        [HttpGet("{id}/materials")]
        public async Task<ActionResult<List<ObjectMaterialDto>>> GetObjectMaterials(int id)
        {
            var obj = await _context.Objects
                .Include(o => o.ObjectMaterials)
                    .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(o => o.IdObject == id);
            if (obj == null) return NotFound();

            var materials = obj.ObjectMaterials.Select(om => new ObjectMaterialDto
            {
                IdObjectMaterial = om.IdObjectMaterial,
                MaterialId = om.MaterialId,
                MaterialName = om.Material?.Name,
                Percentage = om.Percentage,
                Mass = om.Mass,
                Volume = om.Volume
            }).ToList();

            return Ok(materials);
        }

        // POST: api/environmentobjects/{id}/materials
        [HttpPost("{id}/materials")]
        public async Task<IActionResult> AddMaterialToObject(int id, ObjectMaterialCreateDto dto)
        {
            var obj = await _context.Objects.FindAsync(id);
            if (obj == null) return NotFound();

            var material = await _context.Materials.FindAsync(dto.MaterialId);
            if (material == null) return BadRequest("Material not found");

            var objectMaterial = new ObjectMaterial
            {
                ObjectId = id,
                MaterialId = dto.MaterialId,
                Percentage = dto.Percentage,
                Mass = dto.Mass,
                Volume = dto.Volume
            };
            _context.ObjectMaterials.Add(objectMaterial);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/environmentobjects/{id}/materials/{materialId}
        [HttpDelete("{id}/materials/{materialId}")]
        public async Task<IActionResult> RemoveMaterialFromObject(int id, int materialId)
        {
            var objectMaterial = await _context.ObjectMaterials
                .FirstOrDefaultAsync(om => om.ObjectId == id && om.MaterialId == materialId);
            if (objectMaterial == null) return NotFound();

            _context.ObjectMaterials.Remove(objectMaterial);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}