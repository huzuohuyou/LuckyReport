using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace LuckyReport.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FiltersController : ControllerBase
    {
        private readonly LuckyReportContext _context;

        public FiltersController(LuckyReportContext context)
        {
            _context = context;
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

            return NoContent();
        }

        private bool FilterExists(int id)
        {
            return (_context.Filters?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
