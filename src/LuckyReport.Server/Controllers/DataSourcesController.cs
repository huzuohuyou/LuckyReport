using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LuckyReport.Server.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class DataSourcesController : ControllerBase
{
    private readonly LuckyReportContext _context;

    public DataSourcesController(LuckyReportContext context)
    {
        _context = context;
    }

    // GET: api/DataSources
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DataSource>>> GetDataSources()
    {
        return await _context.DataSources!.ToListAsync();
    }

    // GET: api/DataSources/5
    [HttpGet("{id}")]
    public async Task<ActionResult<DataSource>> GetDataSource(int id)
    {
        var dataSource = await _context.DataSources!.FindAsync(id);

        if (dataSource == null)
        {
            return NotFound();
        }

        return dataSource;
    }

    // PUT: api/DataSources/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutDataSource(int id, DataSource dataSource)
    {
        //if (id != dataSource.Id)
        //{
        //    return BadRequest();
        //}

        _context.Entry(dataSource).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DataSourceExists(id))
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

    // POST: api/DataSources
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<DataSource>> PostDataSource(DataSource dataSource)
    {
        if (_context.DataSources != null) _context.DataSources.Add(dataSource);
        await _context.SaveChangesAsync();

        return Ok();
    }

    // DELETE: api/DataSources/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDataSource(int id)
    {
        var dataSource = await _context.DataSources!.FindAsync(id);
        if (dataSource == null)
        {
            return NotFound();
        }

        _context.DataSources.Remove(dataSource);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool DataSourceExists(int id)
    {
        return _context.DataSources!.Any(e => e.Id == id);
    }
}