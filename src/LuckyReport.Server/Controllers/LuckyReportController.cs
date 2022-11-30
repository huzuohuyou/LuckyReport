using LuckSheet_.NetCore.Helper;
using LuckyReport.Server.Helper;
using LuckyReport.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace LuckyReport.Server.Controllers
{
    public class LuckyReportController : ControllerBase
    {
        private Dictionary<string,string> _dataSourceDictionary=new ();
        private readonly Report _report;
        private readonly DataSourceHelper _dataSourceHelper;
        public LuckyReportController(Report report, DataSourceHelper dataSourceHelper)
        {
            _report = report;
            _dataSourceHelper= dataSourceHelper;
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
            //var strDatasource =await _dataSourceHelper.GetDataSource("");// $@"{{""datasource"":{JsonConvert.SerializeObject(await new swaggerClient("https://localhost:7031/", new HttpClient()).GetWeatherForecastAsync())}}}";
            //报表解析
            var jsonObject = JsonNode.Parse(r!.Doc);
            
            await InitData(jsonObject!);
            return jsonObject!.ToString();
        }

        [HttpGet("/reports/{id}/excel", Name = nameof(Excel))]
        [Produces("application/json")]
        public async Task<string> Excel([FromRoute] [Required] int id)
        {
            await using var db = new LuckyReportContext();
            //数据源获取
            var r = await db.Reports.Where(r => r.Id.Equals(id)).FirstOrDefaultAsync();
            var strDatasource = $@"{{""datasource"":{JsonConvert.SerializeObject(await new swaggerClient("https://localhost:7031/", new HttpClient()).GetWeatherForecastAsync())}}}";
            //报表解析
            var jsonObject = JsonNode.Parse(r!.Doc);

            await InitData(jsonObject!, strDatasource);
            var book = ExcelHepler.GenerateExcelStyle(jsonObject!.ToString());
            var excelPath= ExcelHepler.GenerateExcelData(book, jsonObject!.ToString());
            return $@"https://localhost:7103/StaticFiles/{excelPath}";
        }

        /// <summary>
        /// 数据源初始化
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task<string> InitDataSource(string path)
        {
            if(string.IsNullOrWhiteSpace(path)) return string.Empty;
            var match = Regex.Match(path, @"(?<txt>(?<=\$\.).+(?=\[))");//单个值匹配
            if (!match.Success)
                match = Regex.Match(path, @"(?<txt>(?<=\$\.).+(?=\.))");//table匹配
            if (!match.Success)//都没有匹配到值
                return string.Empty;//throw new Exception("没有匹配到数据源");
            var name = match.Value;
            if (!_dataSourceDictionary.ContainsKey(name))
            {
                var value = await _dataSourceHelper.GetDataSource(name);
                if (string.IsNullOrWhiteSpace(value)) throw new Exception("数据源为空");
                _dataSourceDictionary.Add(name, value);
            }
            return _dataSourceDictionary[name];
        }

        private async Task InitData(JsonNode doc, string dataSource="")
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
                    dataSource = await InitDataSource(path);
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

        //private void InitCellData(JsonNode doc, string dataSource)
        //{
        //    var rows = doc.AsArray()[0]!["celldata"]!.AsArray();
        //    for (int columnIndex = 0; columnIndex < rows[0]!.AsArray().Count; columnIndex++)
        //    {
        //        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        //        {
        //            var cell = rows[rowIndex]![columnIndex];
        //            if (cell is null)
        //                continue;
        //            var path = cell["m"]?.ToString();
        //            if (string.IsNullOrWhiteSpace(path))
        //                continue;
        //            var index = 0;
        //            if (path.Contains('#'))
        //                FillTable(ref rowIndex, ref index, columnIndex, path, dataSource, rows, cell);
        //            else
        //            {//单个值数据填充
        //                var value = JsonHelper.GetValue(path, dataSource);
        //                if (value.ok)
        //                {
        //                    rows[rowIndex + index]![columnIndex]!["v"] = value.value;
        //                    rows[rowIndex + index]![columnIndex]!["m"] = value.value;
        //                }
        //            }
        //            rowIndex += index;
        //        }
        //    }
        //}
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
