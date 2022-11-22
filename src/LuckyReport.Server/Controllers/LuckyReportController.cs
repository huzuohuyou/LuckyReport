using LuckyReport.Server.Helper;
using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LuckyReport.Server.Controllers
{
    public class LuckyReportController : ControllerBase
    {

        private readonly Report _report;
        public LuckyReportController(Report report)
        {
            _report = report;
        }
        [HttpPost("/get", Name = nameof(GetDoc))]
        public Task<string> GetDoc()
        {
            return Task.FromResult(_report.Doc);
        }

        [HttpPost("/set", Name = nameof(SetDoc))]
        public async Task<bool> SetDoc(string jsonExcel)
        {
            _report.Doc = jsonExcel;
            await using var db = new LuckyReportContext();

            db.Add(new Report() { Title = "first", Doc = jsonExcel });
            var c = await db.SaveChangesAsync();
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
                r!.Doc = report.Doc;
                var c = await db.SaveChangesAsync();
                Console.WriteLine(c == 1);
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
            //数据源获取
            var r = await db.Reports.Where(r => r.Id.Equals(id)).FirstOrDefaultAsync();
            //var strDatasource =JsonConvert.SerializeObject(await new swaggerClient("https://localhost:7103/",new HttpClient()).GetWeatherForecastAsync());
            //var dataSource = JsonObject.Parse(strDatasource);

            ////报表解析
            //var jsonObject = JsonObject.Parse(r.Doc);

            //InitData(jsonObject, dataSource);
            return r!.Doc;
        }

        [HttpPost("/reports/{id}/view", Name = nameof(View))]
        public async Task<string> View([FromRoute][Required] int id)
        {
            await using var db = new LuckyReportContext();
            //数据源获取
            var r = await db.Reports.Where(r => r.Id.Equals(id)).FirstOrDefaultAsync();
            var strDatasource = $@"{{""datasource"":{JsonConvert.SerializeObject(await new swaggerClient("https://localhost:7103/", new HttpClient()).GetWeatherForecastAsync())}}}";
            //var dataSource = JsonObject.Parse(strDatasource);

            //报表解析
            var jsonObject = JsonNode.Parse(r!.Doc);

            InitData(jsonObject!, strDatasource);
            return jsonObject!.ToString();
        }

        private void InitData(JsonNode doc, string dataSource)
        {
            var rows = doc.AsArray()[0]!["data"]!.AsArray();
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                var cells = row!.AsArray();
                for (int cellIndex = 0; cellIndex < cells.Count; cellIndex++)
                {
                    var cell = row[cellIndex];
                    if (cell is null)
                        continue;
                    var path = cell["m"]?.ToString();
                    if (string.IsNullOrWhiteSpace(path))
                        continue;
                    var index = 0;
                    var value = "";
                    do
                    {
                        if (string.IsNullOrWhiteSpace(cell.ToJsonString()))
                            continue;
                        var sampleJson = System.Text.Encoding.Default.GetBytes(cell.ToJsonString()).AsSpan();
                        var reader = new Utf8JsonReader(sampleJson);
                        var copyNode = JsonObject.Create(JsonElement.ParseValue(ref reader));
                        copyNode!["m"] = null;
                        var temp = path.Replace("#", $@"{index}");
                        value = JsonHelper.GetValue(temp, dataSource);
                        if (string.IsNullOrWhiteSpace(value))
                            break;
                        try
                        {
                            copyNode["v"] = value;
                            rows[rowIndex + index]![cellIndex] = copyNode;
                            index++;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    } while (!string.IsNullOrWhiteSpace(value));
                    cellIndex = cellIndex + index;
                }
            }
        }
    }
}
