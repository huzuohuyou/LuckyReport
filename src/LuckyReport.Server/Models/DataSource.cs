namespace LuckyReport.Server.Models;

public class DataSource
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Uri { get; set; }
    //public string[]? Params { get; set; }
    
    public override string? ToString()
    {
        return Uri;
        //return null == Params ?Uri: string.Format(Uri, Params);
    }
}
