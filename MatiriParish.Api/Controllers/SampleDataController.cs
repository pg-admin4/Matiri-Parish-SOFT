using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatiriParish.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MatiriParish.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SampleDataController : ControllerBase
    {
        private readonly IDbContextFactory<MatiriParishDbContext> _dbFactory;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;

        public SampleDataController(IDbContextFactory<MatiriParishDbContext> dbFactory, IHostEnvironment env, IConfiguration config)
        {
            _dbFactory = dbFactory;
            _env = env;
            _config = config;
        }

        private bool IsAllowed()
        {
            // Allow only in Development or when Seed:Enable = true
            var seedEnabled = _config.GetValue<bool?>("Seed:Enable") ?? false;
            return _env.IsDevelopment() || seedEnabled;
        }

        [HttpGet("users")]
        public async Task<IActionResult> ListSampleUsers()
        {
            if (!IsAllowed()) return Forbid();

            using var db = _dbFactory.CreateDbContext();
            var users = await db.Set<MatiriParish.Core.Models.User>()
                .Where(u => EF.Functions.Like(u.Username, "dev_%") || u.Username == "dev_admin")
                .Select(u => new { u.Id, u.Username, Role = (int)u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("persons")]
        public async Task<IActionResult> ListSamplePersons()
        {
            if (!IsAllowed()) return Forbid();

            using var db = _dbFactory.CreateDbContext();
            var persons = await db.Persons
                .Where(p => EF.Functions.Like(p.FirstName, "DEV_%") || db.Baptisms.Any(b => b.PersonId == p.Id && EF.Functions.Like(b.Remarks ?? string.Empty, "%sample:dev%")))
                .Select(p => new { p.Id, p.FirstName, p.LastName, p.DateOfBirth })
                .ToListAsync();
            return Ok(persons);
        }

        [HttpDelete("user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAllowed()) return Forbid();

            using var db = _dbFactory.CreateDbContext();
            var user = await db.Set<MatiriParish.Core.Models.User>().FindAsync(id);
            if (user == null) return NotFound();
            if (!(user.Username.StartsWith("dev_") || user.Username == "dev_admin")) return BadRequest("Not a sample user");

            db.Set<MatiriParish.Core.Models.User>().Remove(user);
            await db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("person/{id}")]
        public async Task<IActionResult> DeletePerson(int id)
        {
            if (!IsAllowed()) return Forbid();

            using var db = _dbFactory.CreateDbContext();
            var person = await db.Persons.Include(p => p.Baptism).Include(p => p.Confirmation).FirstOrDefaultAsync(p => p.Id == id);
            if (person == null) return NotFound();

            // Only allow deletion for sample markers
            bool isSample = person.FirstName.StartsWith("DEV_") || (person.Baptism != null && (person.Baptism.Remarks ?? string.Empty).Contains("sample:dev"));
            if (!isSample) return BadRequest("Not a sample person");

            // Remove dependent records in safe order
            var baptism = await db.Baptisms.FirstOrDefaultAsync(b => b.PersonId == person.Id);
            if (baptism != null) db.Baptisms.Remove(baptism);

            var confirmation = await db.Confirmations.FirstOrDefaultAsync(c => c.PersonId == person.Id);
            if (confirmation != null) db.Confirmations.Remove(confirmation);

            var marriages = await db.Marriages.Where(m => m.GroomId == person.Id || m.BrideId == person.Id).ToListAsync();
            if (marriages.Any()) db.Marriages.RemoveRange(marriages);

            var death = await db.DeathRegistries.FirstOrDefaultAsync(d => d.PersonId == person.Id);
            if (death != null) db.DeathRegistries.Remove(death);

            db.Persons.Remove(person);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}
