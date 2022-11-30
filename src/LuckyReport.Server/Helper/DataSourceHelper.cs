using Newtonsoft.Json;

namespace LuckyReport.Server.Helper
{
    public  class DataSourceHelper
    {
        private readonly LuckyReportContext _context;

        public DataSourceHelper(LuckyReportContext context)
        {
            _context = context;
        }
        public async Task<string> GetDataSource(string name)
        {
            if (_context.DataSources == null) throw new Exception("找不到数据源");
            var dataSource = await _context.DataSources.SingleAsync(r => r.Name.Equals(name));
            if (dataSource == null) throw new Exception("找不到数据源");
            var result = await new HttpClient().GetStringAsync(dataSource.Uri);
            return $@"{{""{name}"":{result}}}"; ;
        }

        public T GetResponse<T>(string url) where T : class, new()
        {
            T result = default(T);

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    Task<string> t = response.Content.ReadAsStringAsync();
                    string s = t.Result;
                    string json = JsonConvert.DeserializeObject(s).ToString();
                    result = JsonConvert.DeserializeObject<T>(json);
                }
            }
            return result;
        }

       
    }
}
