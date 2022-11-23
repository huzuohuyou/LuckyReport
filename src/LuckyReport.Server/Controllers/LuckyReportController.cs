using LuckyReport.Server.Helper;
using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using LuckSheet_.NetCore.Helper;

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
            //InitCellData(jsonObject!, strDatasource);
            ExcelHepler.GenerateExcel(jsonObject!.ToString());
            return jsonObject!.ToString();
        }

       
        private void InitData(JsonNode doc, string dataSource)
        {
            var rows = doc.AsArray()[0]!["data"]!.AsArray();
            for (int columnIndex = 0; columnIndex < rows[0]!.AsArray().Count; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var cell = rows[rowIndex]![columnIndex];
                    if (cell is null)
                        continue;
                    var path = cell["m"]?.ToString();
                    if (string.IsNullOrWhiteSpace(path))
                        continue;
                    var index = 0;
                    if (path.Contains('#'))
                        FillTable(ref rowIndex, ref index, columnIndex, path, dataSource, rows, cell);
                    else
                    {//单个值数据填充
                        var value = JsonHelper.GetValue(path, dataSource);
                        if (value.ok)
                        {
                            rows[rowIndex + index]![columnIndex]!["v"] = value.value;
                            rows[rowIndex + index]![columnIndex]!["m"] = value.value;
                        }
                    }
                    rowIndex += index;
                }
            }
        }

        private void InitCellData(JsonNode doc, string dataSource)
        {
            var rows = doc.AsArray()[0]!["celldata"]!.AsArray();
            for (int columnIndex = 0; columnIndex < rows[0]!.AsArray().Count; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
                {
                    var cell = rows[rowIndex]![columnIndex];
                    if (cell is null)
                        continue;
                    var path = cell["m"]?.ToString();
                    if (string.IsNullOrWhiteSpace(path))
                        continue;
                    var index = 0;
                    if (path.Contains('#'))
                        FillTable(ref rowIndex, ref index, columnIndex, path, dataSource, rows, cell);
                    else
                    {//单个值数据填充
                        var value = JsonHelper.GetValue(path, dataSource);
                        if (value.ok)
                        {
                            rows[rowIndex + index]![columnIndex]!["v"] = value.value;
                            rows[rowIndex + index]![columnIndex]!["m"] = value.value;
                        }
                    }
                    rowIndex += index;
                }
            }
        }
        /// <summary>
        /// 表格数据填充
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="index"></param>
        /// <param name="columnIndex"></param>
        /// <param name="path"></param>
        /// <param name="dataSource"></param>
        /// <param name="rows"></param>
        /// <param name="cell"></param>
        private static void FillTable(ref int rowIndex,ref int index,int columnIndex,string path,string dataSource, JsonArray rows,JsonNode cell)
        {
            
            var value = "";
            do
            {
                if (string.IsNullOrWhiteSpace(cell.ToJsonString()))
                    continue;
                var copyNode = CopyNode(cell);
                //copyNode["m"] = null;
                var temp = path.Replace("#", $@"{index}");
                value = JsonHelper.GetValue(temp, dataSource).value;
                if (string.IsNullOrWhiteSpace(value))
                    break;
                try
                {
                    copyNode["v"] = value;
                    copyNode["m"] = value;
                    rows[rowIndex + index]![columnIndex] = copyNode;
                    index++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            } while (!string.IsNullOrWhiteSpace(value));
        }


        private static JsonNode CopyNode(JsonNode node)
        {
            var sampleJson = System.Text.Encoding.Default.GetBytes(node.ToJsonString()).AsSpan();
            var reader = new Utf8JsonReader(sampleJson);
            var copyNode = JsonObject.Create(JsonElement.ParseValue(ref reader));
            return copyNode!;
        }
    }
}
