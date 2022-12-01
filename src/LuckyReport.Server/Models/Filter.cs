namespace LuckyReport.Server.Models;

public class Filter
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Field { get; set; }
    public FilterType Type { get; set; }
    public string? DataSource { get; set; }
    public Filter(string title, string field, FilterType type)
    {
        Title = title;
        Field = field;
        Type = type;
    }

}

public enum FilterType
{
    DATE,
    COMBOX,
    TEXT
}