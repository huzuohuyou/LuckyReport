using System.Net.Http;

namespace LuckyReport.Helpers
{
    public class LuckyReportHelper
    {
        //private readonly IHttpClientFactory _httpClientFactory;
        public readonly swaggerClient SwaggerClient;

        //private readonly swaggerClient swaggerClient;
        public LuckyReportHelper(IHttpClientFactory httpClientFactory)
        {
            SwaggerClient = new swaggerClient("https://localhost:7103/", httpClientFactory.CreateClient());
        }



    }
}
