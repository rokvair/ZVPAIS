using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API;
using ŽVPAIS_API.Models;

namespace Zpvis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MaterialsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaterialsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialResponseDto>>> GetMaterials()
        {
            var materials = await _context.Materials.ToListAsync();
            var dtos = materials.Select(m => new MaterialResponseDto
            {
                IdMaterial = m.IdMaterial,
                Name = m.Name,
                Description = m.Description,
                ToxicityFactor = m.ToxicityFactor,
                Unit = m.Unit,
                BaseRate = m.BaseRate,
                SubstanceType = m.SubstanceType,
                CreatedAt = m.CreatedAt
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialResponseDto>> GetMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return NotFound();

            var dto = new MaterialResponseDto
            {
                IdMaterial = material.IdMaterial,
                Name = material.Name,
                Description = material.Description,
                ToxicityFactor = material.ToxicityFactor,
                Unit = material.Unit,
                BaseRate = material.BaseRate,
                SubstanceType = material.SubstanceType,
                CreatedAt = material.CreatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<MaterialResponseDto>> CreateMaterial(MaterialCreateDto dto)
        {
            var material = new Material
            {
                Name = dto.Name,
                Description = dto.Description,
                ToxicityFactor = dto.ToxicityFactor,
                Unit = dto.Unit,
                BaseRate = dto.BaseRate,
                SubstanceType = dto.SubstanceType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            var responseDto = new MaterialResponseDto
            {
                IdMaterial = material.IdMaterial,
                Name = material.Name,
                Description = material.Description,
                ToxicityFactor = material.ToxicityFactor,
                Unit = material.Unit,
                BaseRate = material.BaseRate,
                SubstanceType = material.SubstanceType,
                CreatedAt = material.CreatedAt
            };

            return CreatedAtAction(nameof(GetMaterial), new { id = material.IdMaterial }, responseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> UpdateMaterial(int id, MaterialCreateDto dto)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return NotFound();

            material.Name = dto.Name;
            material.Description = dto.Description;
            material.ToxicityFactor = dto.ToxicityFactor;
            material.Unit = dto.Unit;
            material.BaseRate = dto.BaseRate;
            material.SubstanceType = dto.SubstanceType;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Specialist")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
                return NotFound();

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}