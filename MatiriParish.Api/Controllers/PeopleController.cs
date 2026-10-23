using MatiriParish.Core.Data;
using MatiriParish.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatiriParish.Api.Controllers;

n[ApiController]
[Route("api/[controller]")]
public class PeopleController : ControllerBase
{
    private readonly IDbContextFactory<MatiriParishDbContext> _dbFactory;

n    public PeopleController(IDbContextFactory<MatiriParishDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

n    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        await using var db = _dbFactory.CreateDbContext();
        var list = await db.Persons.AsNoTracking().ToListAsync();
        return Ok(list);
    }

n    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        await using var db = _dbFactory.CreateDbContext();
        var p = await db.Persons.FindAsync(id);
        if (p == null) return NotFound();
        return Ok(p);
    }

n    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Person person)
    {
        await using var db = _dbFactory.CreateDbContext();
        db.Persons.Add(person);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = person.Id }, person);
    }

n    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Person updated)
    {
        if (id != updated.Id) return BadRequest();
        await using var db = _dbFactory.CreateDbContext();
        db.Persons.Update(updated);
        await db.SaveChangesAsync();
        return NoContent();
    }

n    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await using var db = _dbFactory.CreateDbContext();
        var p = await db.Persons.FindAsync(id);
        if (p == null) return NotFound();
        db.Persons.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
