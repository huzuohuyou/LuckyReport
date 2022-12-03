using Microsoft.Extensions.Hosting;

namespace LuckyReport.Server.Models;

public class DataSource
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Uri { get; set; }
    public List<Filter> Filters { get; set; }
    public override string? ToString()
    {
        return Uri;
        //return null == Params ?Uri: string.Format(Uri, Params);
    }
}
