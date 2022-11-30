using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuckyReport.Server.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly LuckyReportContext _context;

        public ReportsController(LuckyReportContext context)
        {
            _context = context;
        }

        // GET: api/Reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
          if (_context.Reports == null)
          {
              return NotFound();
          }
            return await _context.Reports.ToListAsync();
        }

        // GET: api/Reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
          if (_context.Reports == null)
          {
              return NotFound();
          }
            var report = await _context.Reports.FindAsync(id);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        // PUT: api/Reports/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, [FromBody] Report report)
        {
            if (id != report.Id)
            {
                return BadRequest();
            }

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
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

        // POST: api/Reports
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Report>> PostReport([FromBody]Report report)
        {
          if (_context.Reports == null)
          {
              return Problem("Entity set 'LuckyReportContext.Reports'  is null.");
          }
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Ok();
        }

        //[HttpPost]
        //public async Task<ActionResult<DataSource>> PostDataSource(DataSource dataSource)
        //{
        //    if (_context.DataSources == null)
        //    {
        //        return Problem("Entity set 'LuckyReportContext.DataSources'  is null.");
        //    }
        //    _context.DataSources.Add(dataSource);
        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}

        // DELETE: api/Reports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            if (_context.Reports == null)
            {
                return NotFound();
            }
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReportExists(int id)
        {
            return (_context.Reports?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
