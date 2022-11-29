namespace LuckyReport.Server.Models;

public class DataSource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Uri { get; set; }
    //public string[]? Params { get; set; }
    public DataSource()
    {
    }

    public DataSource(string uri, string name)
    {
        Uri = uri;
        Name = name;
    }

    public override string ToString()
    {
        return Uri;
        //return null == Params ?Uri: string.Format(Uri, Params);
    }
}
