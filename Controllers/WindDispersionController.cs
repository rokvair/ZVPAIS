using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ŽVPAIS_API.Services;

namespace ŽVPAIS_API.Controllers
{
    [ApiController]
    [Route("api/wind-dispersion")]
    [Authorize]
    public class WindDispersionController : ControllerBase
    {
        private readonly IGaussianPlumeService _plume;

        public WindDispersionController(IGaussianPlumeService plume)
        {
            _plume = plume;
        }

        /// <summary>Returns the list of waste types for the dispersion form selector.</summary>
        [HttpGet("waste-types")]
        public async Task<IActionResult> GetWasteTypes()
        {
            var list = await _plume.GetWasteTypesAsync();
            return Ok(list);
        }

        /// <summary>Calculates Gaussian plume dispersion for a manually specified waste type and fire parameters.</summary>
        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] DispersionRequestDto request)
        {
            try
            {
                var result = await _plume.CalculateAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Calculates dispersion for an existing event, auto-deriving emission rates from the event's linked object materials.</summary>
        [HttpPost("calculate-from-event/{eventId}")]
        public async Task<IActionResult> CalculateFromEvent(int eventId, [FromBody] WindParamsDto wind)
        {
            try
            {
                var result = await _plume.CalculateFromEventAsync(eventId, wind);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
