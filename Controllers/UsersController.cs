using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace Zpvis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Grąžiname be slaptažodžių (saugumo sumetimais)
            var users = await _context.Users
                .Select(u => new { u.IdUser, u.Email, u.CreatedAt })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Specialist)
                .FirstOrDefaultAsync(u => u.IdUser == id);

            if (user == null)
                return NotFound();

            // Paslepiame slaptažodį
            user.Password = null;

            return Ok(user);
        }

        // POST būtų registracija – čia galima pridėti
        // DELETE – tik administratoriams
    }
}