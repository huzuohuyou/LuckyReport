using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LuckyReport.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltersController : ControllerBase
    {
        private readonly LuckyReportContext _context;
        private readonly IMemoryCache _memoryCache;
        public FiltersController(LuckyReportContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        // GET: api/Filters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Filter>>> GetFilters()
        {
          if (_context.Filters == null)
          {
              return NotFound();
          }
            return await _context.Filters.ToListAsync();
        }

        // GET: api/Filters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Filter>> GetFilter(int id)
        {
          if (_context.Filters == null)
          {
              return NotFound();
          }
            var filter = await _context.Filters.FindAsync(id);

            if (filter == null)
            {
                return NotFound();
            }

            return filter;
        }

        // PUT: api/Filters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFilter(int id, [FromBody]Filter filter)
        {
            if (id != filter.Id)
            {
                return BadRequest();
            }

            _context.Entry(filter).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FilterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Filters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Filter>> PostFilter([FromBody]Filter filter)
        {
          if (_context.Filters == null)
          {
              return Problem("Entity set 'LuckyReportContext.Filters'  is null.");
          }
            _context.Filters.Add(filter);
            await _context.SaveChangesAsync();
            return Ok();
            //return CreatedAtAction("GetFilter", new { id = filter.Id }, filter);
        }

        /// <summary>
        /// 缓存过滤条件
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        [HttpPost("Caches/{requestId}")]
        public IActionResult CacheFilter([FromRoute] string requestId, [FromBody] List<Filter> filters)
        {
            if (!_memoryCache.TryGetValue(requestId, out List<Filter>? cacheValue))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(30));
                _memoryCache.Set(requestId, cacheValue, cacheEntryOptions);
            }
            return Ok();
        }

        // DELETE: api/Filters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFilter(int id)
        {
            if (_context.Filters == null)
            {
                return NotFound();
            }
            var filter = await _context.Filters.FindAsync(id);
            if (filter == null)
            {
                return NotFound();
            }

            _context.Filters.Remove(filter);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool FilterExists(int id)
        {
            return (_context.Filters?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
