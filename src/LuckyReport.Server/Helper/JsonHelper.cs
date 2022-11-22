using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;

namespace LuckyReport.Server.Helper
{
    public static class JsonHelper
    {
        public static string GetValue(string key, string json)
        {
            try
            {
                var rkey = $@"$.datasource.{key.ToLower()}";//一般属性模型
                if (key.StartsWith('['))//数组模型
                    rkey = $@"$.datasource{key.ToLower()}";
                JObject obj = JObject.Parse(json.ToLower());
                JToken age = obj.SelectToken(rkey);
                if (age != null)
                    return age.Value<string>()!;
                return string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return string.Empty;
            }

        }
    }
}
