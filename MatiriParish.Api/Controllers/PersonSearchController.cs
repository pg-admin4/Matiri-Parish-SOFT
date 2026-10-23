using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MatiriParish.Api.Data;
using MatiriParish.Api.Models;

namespace MatiriParish.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonSearchController : ControllerBase
    {
        private readonly MatiriParishDbContext _db;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _memoryCache;
        private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache? _distributedCache;

        public PersonSearchController(MatiriParishDbContext db, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, Microsoft.Extensions.Caching.Distributed.IDistributedCache? distributedCache = null)
        {
            _db = db;
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
        }

        [HttpGet("baptism")]
        public async Task<ActionResult<List<PersonSummaryDto>>> SearchBaptism([FromQuery] string first, [FromQuery] string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                return BadRequest("first and last query parameters are required");

            var list = await _db.Baptisms
                .Include(b => b.Person)
                .Where(b => b.Person.FirstName.ToLower() == first.ToLower() && b.Person.LastName.ToLower() == last.ToLower())
                .Select(b => new PersonSummaryDto
                {
                    Id = b.Person.Id,
                    FirstName = b.Person.FirstName,
                    MiddleName = b.Person.MiddleName,
                    LastName = b.Person.LastName,
                    DateOfBirth = b.Person.DateOfBirth,
                    FatherName = b.Person.Father != null ? (b.Person.Father.FirstName + " " + b.Person.Father.LastName) : string.Empty,
                    MotherName = b.Person.Mother != null ? (b.Person.Mother.FirstName + " " + b.Person.Mother.LastName) : string.Empty,
                    BaptismBook = b.BookNumber,
                    BaptismPage = b.PageNumber,
                    BaptismEntry = b.EntryNumber
                })
                .ToListAsync();

            return Ok(list);
        }

        // New paged endpoint to support server-side paging for large result sets
        [HttpGet("baptism/paged")]
        public async Task<ActionResult<Models.PagedResult<PersonSummaryDto>>> SearchBaptismPaged([FromQuery] string first, [FromQuery] string last, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                return BadRequest("first and last query parameters are required");

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var cacheKey = $"baptism:paged:{first}:{last}:{page}:{pageSize}".ToLowerInvariant();

            // Try memory cache first
            if (_memoryCache.TryGetValue(cacheKey, out Models.PagedResult<PersonSummaryDto>? cached))
            {
                return Ok(cached);
            }

            // Try distributed cache (Redis) if available
            if (_distributedCache != null)
            {
                var bytes = await _distributedCache.GetAsync(cacheKey);
                if (bytes != null && bytes.Length > 0)
                {
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize<Models.PagedResult<PersonSummaryDto>>(json);
                    if (deserialized != null)
                    {
                        // warm memory cache
                        _memoryCache.Set(cacheKey, deserialized, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
                        return Ok(deserialized);
                    }
                }
            }

            // Build base query (projection avoids loading entire graph and prevents N+1 issues)
            var baseQuery = _db.Baptisms
                .Where(b => EF.Functions.ILike(b.Person.FirstName, first) && EF.Functions.ILike(b.Person.LastName, last))
                .Select(b => new PersonSummaryDto
                {
                    Id = b.Person.Id,
                    FirstName = b.Person.FirstName,
                    MiddleName = b.Person.MiddleName,
                    LastName = b.Person.LastName,
                    DateOfBirth = b.Person.DateOfBirth,
                    FatherName = b.Person.Father != null ? (b.Person.Father.FirstName + " " + b.Person.Father.LastName) : string.Empty,
                    MotherName = b.Person.Mother != null ? (b.Person.Mother.FirstName + " " + b.Person.Mother.LastName) : string.Empty,
                    BaptismBook = b.BookNumber,
                    BaptismPage = b.PageNumber,
                    BaptismEntry = b.EntryNumber
                });

            var total = await baseQuery.LongCountAsync();

            var items = await baseQuery
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new Models.PagedResult<PersonSummaryDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                Items = items
            };

            // Cache short-lived results
            _memoryCache.Set(cacheKey, result, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });

            if (_distributedCache != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                await _distributedCache.SetAsync(cacheKey, bytes, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return Ok(result);
        }

        [HttpGet("firstholycommunion")]
        public async Task<ActionResult<List<PersonSummaryDto>>> SearchFirstHolyCommunion([FromQuery] string first, [FromQuery] string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                return BadRequest("first and last query parameters are required");

            var list = await _db.FirstHolyCommunions
                .Include(f => f.Person)
                .ThenInclude(p => p.Baptism)
                .Where(f => f.Person.FirstName.ToLower() == first.ToLower() && f.Person.LastName.ToLower() == last.ToLower())
                .Select(f => new PersonSummaryDto
                {
                    Id = f.Person.Id,
                    FirstName = f.Person.FirstName,
                    MiddleName = f.Person.MiddleName,
                    LastName = f.Person.LastName,
                    DateOfBirth = f.Person.DateOfBirth,
                    FatherName = f.Person.Father != null ? (f.Person.Father.FirstName + " " + f.Person.Father.LastName) : string.Empty,
                    MotherName = f.Person.Mother != null ? (f.Person.Mother.FirstName + " " + f.Person.Mother.LastName) : string.Empty,
                    BaptismBook = f.Person.Baptism != null ? f.Person.Baptism.BookNumber : string.Empty,
                    BaptismPage = f.Person.Baptism != null ? f.Person.Baptism.PageNumber : 0,
                    BaptismEntry = f.Person.Baptism != null ? f.Person.Baptism.EntryNumber : 0
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("firstholycommunion/paged")]
        public async Task<ActionResult<Models.PagedResult<PersonSummaryDto>>> SearchFirstHolyCommunionPaged([FromQuery] string first, [FromQuery] string last, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                return BadRequest("first and last query parameters are required");

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var cacheKey = $"fhc:paged:{first}:{last}:{page}:{pageSize}".ToLowerInvariant();
            if (_memoryCache.TryGetValue(cacheKey, out Models.PagedResult<PersonSummaryDto>? cached))
            {
                return Ok(cached);
            }

            if (_distributedCache != null)
            {
                var bytes = await _distributedCache.GetAsync(cacheKey);
                if (bytes != null && bytes.Length > 0)
                {
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize<Models.PagedResult<PersonSummaryDto>>(json);
                    if (deserialized != null)
                    {
                        _memoryCache.Set(cacheKey, deserialized, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
                        return Ok(deserialized);
                    }
                }
            }

            var baseQuery = _db.FirstHolyCommunions
                .Where(f => EF.Functions.ILike(f.Person.FirstName, first) && EF.Functions.ILike(f.Person.LastName, last))
                .Select(f => new PersonSummaryDto
                {
                    Id = f.Person.Id,
                    FirstName = f.Person.FirstName,
                    MiddleName = f.Person.MiddleName,
                    LastName = f.Person.LastName,
                    DateOfBirth = f.Person.DateOfBirth,
                    FatherName = f.Person.Father != null ? (f.Person.Father.FirstName + " " + f.Person.Father.LastName) : string.Empty,
                    MotherName = f.Person.Mother != null ? (f.Person.Mother.FirstName + " " + f.Person.Mother.LastName) : string.Empty,
                    BaptismBook = f.Person.Baptism != null ? f.Person.Baptism.BookNumber : string.Empty,
                    BaptismPage = f.Person.Baptism != null ? f.Person.Baptism.PageNumber : 0,
                    BaptismEntry = f.Person.Baptism != null ? f.Person.Baptism.EntryNumber : 0
                });

            var total = await baseQuery.LongCountAsync();
            var items = await baseQuery.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new Models.PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = total, Items = items };
            _memoryCache.Set(cacheKey, result, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
            if (_distributedCache != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                await _distributedCache.SetAsync(cacheKey, bytes, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return Ok(result);
        }

        [HttpGet("confirmation")]
        public async Task<ActionResult<List<PersonSummaryDto>>> SearchConfirmation([FromQuery] string first, [FromQuery] string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                return BadRequest("first and last query parameters are required");

            var list = await _db.Confirmations
                .Include(c => c.Person)
                .ThenInclude(p => p.Baptism)
                .Where(c => c.Person.FirstName.ToLower() == first.ToLower() && c.Person.LastName.ToLower() == last.ToLower())
                .Select(c => new PersonSummaryDto
                {
                    Id = c.Person.Id,
                    FirstName = c.Person.FirstName,
                    MiddleName = c.Person.MiddleName,
                    LastName = c.Person.LastName,
                    DateOfBirth = c.Person.DateOfBirth,
                    FatherName = c.Person.Father != null ? (c.Person.Father.FirstName + " " + c.Person.Father.LastName) : string.Empty,
                    MotherName = c.Person.Mother != null ? (c.Person.Mother.FirstName + " " + c.Person.Mother.LastName) : string.Empty,
                    BaptismBook = c.Person.Baptism != null ? c.Person.Baptism.BookNumber : string.Empty,
                    BaptismPage = c.Person.Baptism != null ? c.Person.Baptism.PageNumber : 0,
                    BaptismEntry = c.Person.Baptism != null ? c.Person.Baptism.EntryNumber : 0
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet("confirmation/paged")]
        public async Task<ActionResult<Models.PagedResult<PersonSummaryDto>>> SearchConfirmationPaged([FromQuery] string first, [FromQuery] string last, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                return BadRequest("first and last query parameters are required");

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var cacheKey = $"confirmation:paged:{first}:{last}:{page}:{pageSize}".ToLowerInvariant();
            if (_memoryCache.TryGetValue(cacheKey, out Models.PagedResult<PersonSummaryDto>? cached))
            {
                return Ok(cached);
            }

            if (_distributedCache != null)
            {
                var bytes = await _distributedCache.GetAsync(cacheKey);
                if (bytes != null && bytes.Length > 0)
                {
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    var deserialized = System.Text.Json.JsonSerializer.Deserialize<Models.PagedResult<PersonSummaryDto>>(json);
                    if (deserialized != null)
                    {
                        _memoryCache.Set(cacheKey, deserialized, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
                        return Ok(deserialized);
                    }
                }
            }

            var baseQuery = _db.Confirmations
                .Where(c => EF.Functions.ILike(c.Person.FirstName, first) && EF.Functions.ILike(c.Person.LastName, last))
                .Select(c => new PersonSummaryDto
                {
                    Id = c.Person.Id,
                    FirstName = c.Person.FirstName,
                    MiddleName = c.Person.MiddleName,
                    LastName = c.Person.LastName,
                    DateOfBirth = c.Person.DateOfBirth,
                    FatherName = c.Person.Father != null ? (c.Person.Father.FirstName + " " + c.Person.Father.LastName) : string.Empty,
                    MotherName = c.Person.Mother != null ? (c.Person.Mother.FirstName + " " + c.Person.Mother.LastName) : string.Empty,
                    BaptismBook = c.Person.Baptism != null ? c.Person.Baptism.BookNumber : string.Empty,
                    BaptismPage = c.Person.Baptism != null ? c.Person.Baptism.PageNumber : 0,
                    BaptismEntry = c.Person.Baptism != null ? c.Person.Baptism.EntryNumber : 0
                });

            var total = await baseQuery.LongCountAsync();
            var items = await baseQuery.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new Models.PagedResult<PersonSummaryDto> { Page = page, PageSize = pageSize, TotalCount = total, Items = items };
            _memoryCache.Set(cacheKey, result, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromSeconds(30) });
            if (_distributedCache != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(result);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                await _distributedCache.SetAsync(cacheKey, bytes, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return Ok(result);
        }
    }
}
