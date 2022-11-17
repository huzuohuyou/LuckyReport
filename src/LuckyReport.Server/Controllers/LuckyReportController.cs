using System.ComponentModel.DataAnnotations;
using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace LuckyReport.Server.Controllers
{
    public class LuckyReportController : ControllerBase
    {
        
        private readonly Report _report;
        public LuckyReportController(Report report)
        {
            _report=report;
        }
        [HttpPost("/get",Name = nameof(GetDoc))]
        public Task<string> GetDoc() {
            return Task.FromResult(_report.Doc);
        }

        [HttpPost("/set", Name = nameof(SetDoc))]
        public async Task<bool> SetDoc(string jsonExcel)
        {
            _report.Doc=jsonExcel;
            await using var db = new LuckyReportContext();

            db.Add(new Report() { Title = "first",Doc = jsonExcel });
            var c=await db.SaveChangesAsync();
            return c == 1;
        }

        [HttpPost("/reports", Name = nameof(Post))]
        public async Task<bool> Post([FromBody][Required] Report report)
        {
            await using var db = new LuckyReportContext();
            var r =await db.Reports.AddAsync(report);
            var c = await db.SaveChangesAsync();
            return c == 1;
        }

        [HttpGet("/reports", Name = nameof(GetAll))]
        public async Task<IEnumerable<Report>> GetAll()
        {
            await using var db = new LuckyReportContext();
            var r = db.Reports.ToList();
            return r;
        }

        [HttpGet("/reports/{id}", Name = nameof(Get))]
        public async Task<Report?> Get([FromRoute][Required] int id)
        {
            await using var db = new LuckyReportContext();
            var r =await db.Reports.Where(r=>r.Id.Equals(id)).FirstOrDefaultAsync();
            return r;
        }
    }
}
