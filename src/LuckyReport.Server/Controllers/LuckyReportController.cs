using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WebSocketsSample.Models;

namespace WebSocketsSample.Controllers
{
    public class LuckyReportController : ControllerBase
    {
        string doc;
        private readonly Report _report;
        public LuckyReportController(Report report)
        {
            _report=report;
        }
        [HttpPost("/get")]
        public async Task<string> Get() {
            return _report.Doc;
        }

        [HttpPost("/set")]
        public async Task<bool> Set(string jsonExcel)
        {
            _report.Doc=jsonExcel;
            doc = jsonExcel;
            return true;
        }

    }
}
