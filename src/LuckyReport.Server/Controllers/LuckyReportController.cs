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
            try
            {
                report.Title = "new";
                await using var db = new LuckyReportContext();
                var r = await db.Reports.Where(r => r.Id.Equals(report.Id)).FirstOrDefaultAsync();
                r.Doc = report.Doc;
                var c = await db.SaveChangesAsync();
                Console.WriteLine(c==1);
                return c == 1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
           
        }

        [HttpGet("/reports", Name = nameof(GetAll))]
        public async Task<IEnumerable<Report>> GetAll()
        {
            await using var db = new LuckyReportContext();
            var r = db.Reports.ToList();
            return r;
        }

        [HttpPost("/reports/{id}", Name = nameof(Get))]
        public async Task<string> Get([FromRoute][Required] int id)
        {
            await using var db = new LuckyReportContext();
            var r =await db.Reports.Where(r=>r.Id.Equals(id)).FirstOrDefaultAsync();
            return r.Doc;
        }
    }
}
