namespace LuckyReport.Server.Models;

public class Filter
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Field { get; set; }
    public FilterType Type { get; set; }
    public string? Content { get; set; }

    public int DataSourceId { get; set; }
    //public DataSource DataSource { get; set; }

    public Filter()
    {
    }

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