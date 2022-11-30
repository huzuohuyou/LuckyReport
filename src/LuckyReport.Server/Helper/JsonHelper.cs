using Newtonsoft.Json.Linq;

namespace LuckyReport.Server.Helper
{
    public static class JsonHelper
    {
        public static (bool ok,string value) GetValue(string key, string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json)) return new(false, string.Empty);
                //var rkey = $@"$.datasource.{key.ToLower()}";//一般属性模型
                //if (key.StartsWith('['))//数组模型
                //    rkey = $@"$.datasource{key.ToLower()}";
                JObject obj = JObject.Parse(json.ToLower());
                JToken? token = obj.SelectToken(key.ToLower());
                if (token != null)
                    return new (true,token.Value<string>()!);
                return new (false,string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new(false, string.Empty);
            }

        }
    }
}
