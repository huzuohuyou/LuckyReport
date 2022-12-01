using LuckyReport.Server.Models;

namespace LuckyReport.Server.Helper;

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
        var dataSource = await _context.DataSources.SingleAsync(r => name.Equals(r.Name));
        if (dataSource == null) throw new Exception("找不到数据源");
        var result = await new HttpClient().GetStringAsync(dataSource.Uri);
        return $@"{{""{name}"":{result}}}";
    }
}