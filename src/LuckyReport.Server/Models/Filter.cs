using System.ComponentModel.DataAnnotations.Schema;

namespace LuckyReport.Server.Models;

public class Filter
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Field { get; set; }
    public FilterType Type { get; set; }
    public string? Content { get; set; }

    [NotMapped]
    public DateTime?[]? RangePickerValue { get; set; }

    [NotMapped]
    public string? InputValue { get; set; }

    [NotMapped]
    public int? SelectValue { get; set; }
    public int DataSourceId { get; set; }

}

public enum FilterType
{
    DATE,
    COMBOX,
    TEXT
}