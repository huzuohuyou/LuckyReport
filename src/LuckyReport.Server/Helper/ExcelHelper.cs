using Handy.DotNETCoreCompatibility.ColourTranslations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Drawing;

namespace LuckyReport.Server.Helper;

public static class ExcelHelper
{
    

    /// <summary>
    /// 获取边框样式
    /// </summary>
    /// <param name="styleId"></param>
    /// <returns></returns>
    private static BorderStyle GetBorderStyle(int styleId)
    {
        switch (styleId)
        {
            case 0:
                return BorderStyle.None;
            case 1:
                return BorderStyle.Thin;
            case 2:
                return BorderStyle.Medium;
            case 3:
                return BorderStyle.Dashed;
            case 4:
                return BorderStyle.Dotted;
            case 5:
                return BorderStyle.Thick;
            case 6:
                return BorderStyle.Double;
            case 7:
                return BorderStyle.Hair;
            case 8:
                return BorderStyle.MediumDashed;
            case 9:
                return BorderStyle.DashDot;
            case 10:
                return BorderStyle.MediumDashDot;
            case 11:
                return BorderStyle.DashDotDot;
            case 12:
                return BorderStyle.MediumDashDotDot;
            case 13:
                return BorderStyle.SlantedDashDot;
            default:
                return BorderStyle.None;
        }
    }

    [Obsolete("Obsolete")]
    public static IWorkbook GenerateExcelStyle(string strData)
    {
        List<JObject>? dowExcels = JsonConvert.DeserializeObject<List<JObject>>(strData);
        IWorkbook workbook = new HSSFWorkbook();
        //赋予表格默认样式，因为样式文件存储只能有4000个
        ICellStyle cellStyle = workbook.CreateCellStyle();
        cellStyle.VerticalAlignment = VerticalAlignment.Center; //垂直居中
        cellStyle.WrapText = true; //自动换行
        cellStyle.Alignment = HorizontalAlignment.Center; //水平居中
        cellStyle.FillPattern = FillPattern.NoFill; //背景色是必须要这数据
        cellStyle.FillForegroundColor = IndexedColors.White.Index;//默认背景色
        foreach (var item in dowExcels!)
        {
            var columnlen = item["config"]?["columnlen"];
            var rowlen = item["config"]?["rowlen"];
            var borderInfo = item["config"]?["borderInfo"];
            var cjarray = item["celldata"];
            var merge = item["config"]?["merge"];
            //读取了模板内所有sheet内容
            ISheet sheet = workbook.CreateSheet(item["name"]?.ToString());
            //判断是否有值，并且赋予样式
            if (cjarray != null)
            {
                for (int i = 0; i < cjarray.Count(); i++)
                {
                    //判断行，存不存在，不存在创建
                    IRow row = sheet.GetRow(int.Parse(cjarray[i]?["r"]!.ToString()!)) ?? sheet.CreateRow(int.Parse(cjarray[i]?["r"]?.ToString()!));
                    var ct = cjarray[i]?["v"]?["ct"];
                    var cct = cjarray[i]?["v"];
                    if (ct != null && ct["s"] != null)
                    {
                        //合并单元格的走这边
                        string celldatas = "";
                        //合并单元格时，会导致文字丢失，提前处理文字信息
                        for (int j = 0; j < ct["s"]?.Count(); j++)
                        {
                            var stv = ct["s"]?[j];
                            celldatas += stv?["v"] != null ? stv["v"]?.ToString() : "";
                        }
                        //判断列，不存在创建
                        ICell? cell = row.GetCell(int.Parse(cjarray[i]?["c"]?.ToString()!));
                        if (cell == null)
                        {
                            HSSFRichTextString richtext = new HSSFRichTextString(celldatas);
                            cell = row.CreateCell(int.Parse(cjarray[i]?["c"]?.ToString()!));
                            for (int k = 0; k < ct["s"]?.Count(); k++)
                            {
                                IFont font = workbook.CreateFont();
                                var stv = ct["s"]?[k];
                                //文字颜色
                                if (stv?["fc"] != null)
                                {
                                    var rGb = HTMLColorTranslator.Translate(stv["fc"]?.ToString());
                                    var color = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    font.Color = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(color.R, color.G, color.B).Indexed;
                                }
                                else
                                    font.Color = HSSFColor.Black.Index;
                                //是否加粗
                                if (stv?["bl"] != null)
                                {
                                    font.IsBold = !string.IsNullOrEmpty(stv["bl"]?.ToString()) && (stv["bl"]?.ToString() == "1");
                                    font.Boldweight = stv["bl"]?.ToString() == "1" ? (short)FontBoldWeight.Bold : (short)FontBoldWeight.None;
                                }
                                else
                                {
                                    font.IsBold = false;
                                    font.Boldweight = (short)FontBoldWeight.None;
                                }
                                //是否斜体
                                if (stv?["it"] != null)
                                    font.IsItalic = !string.IsNullOrEmpty(stv["it"]?.ToString()) && (stv["it"]?.ToString() == "1");
                                else
                                    font.IsItalic = false;
                                //下划线
                                if (stv?["un"] != null)
                                {
                                    font.Underline = stv["un"]!.ToString() == "1" ? FontUnderlineType.Single : FontUnderlineType.None;
                                }
                                else
                                    font.Underline = FontUnderlineType.None;
                                //字体
                                if (stv?["ff"] != null)
                                    font.FontName = stv["ff"]!.ToString();
                                //文字大小
                                if (stv?["fs"] != null)
                                    font.FontHeightInPoints = double.Parse(stv["fs"]!.ToString());
                                cell.CellStyle.SetFont(font);
                                richtext.ApplyFont(celldatas.IndexOf(stv?["v"]!.ToString()!, StringComparison.Ordinal), celldatas.IndexOf(stv?["v"]!.ToString()!, StringComparison.Ordinal) + stv["v"]!.ToString().Length, font);
                                cell.SetCellValue(richtext);
                            }
                            //背景颜色
                            if (cct?["bg"] != null)
                            {
                                ICellStyle cellStyle1 = workbook.CreateCellStyle();
                                cellStyle1.CloneStyleFrom(cellStyle);
                                if (cct["bg"] != null)
                                {
                                    var rGb = HTMLColorTranslator.Translate(cct["bg"]?.ToString());
                                    var color = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    cellStyle1.FillPattern = FillPattern.SolidForeground;
                                    cellStyle1.FillForegroundColor = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(color.R, color.G, color.B).Indexed;
                                }
                                cell.CellStyle = cellStyle1;
                            }
                            else
                            {
                                cell.CellStyle = cellStyle;
                            }
                        }
                    }
                    else
                    {
                        //没有合并单元格的走这边
                        //判断列，不存在创建
                        ICell cell = row.GetCell(int.Parse(cjarray[i]?["c"]?.ToString()!));
                        if (cell == null)
                        {
                            cell = row.CreateCell(int.Parse(cjarray[i]?["c"]?.ToString()!));
                            IFont font = workbook.CreateFont();
                            ct = cjarray[i]?["v"];
                            //字体颜色
                            if (ct?["fc"] != null)
                            {
                                var rGb = HTMLColorTranslator.Translate(ct["fc"]?.ToString());
                                var color = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                font.Color = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(color.R, color.G, color.B).Indexed;
                            }
                            else
                                font.Color = HSSFColor.Black.Index;
                            //是否加粗
                            if (ct?["bl"] != null)
                            {
                                font.IsBold = !string.IsNullOrEmpty(ct["bl"]?.ToString()) && (ct["bl"]?.ToString() == "1");
                                font.Boldweight = ct["bl"]?.ToString() == "1" ? (short)FontBoldWeight.Bold : (short)FontBoldWeight.None;
                            }
                            else
                            {
                                font.IsBold = false;
                                font.Boldweight = (short)FontBoldWeight.None;
                            }
                            //斜体
                            if (ct?["it"] != null)
                                font.IsItalic = !string.IsNullOrEmpty(ct["it"]?.ToString()!) && (ct["it"]?.ToString() == "1");
                            else
                                font.IsItalic = false;
                            //下划线
                            if (ct?["un"] != null)
                                font.Underline = ct["un"]?.ToString()! == "1" ? FontUnderlineType.Single : FontUnderlineType.None;
                            else
                                font.Underline = FontUnderlineType.None;
                            //字体
                            if (ct?["ff"] != null)
                                font.FontName = ct["ff"]?.ToString()!;
                            //文字大小
                            if (ct?["fs"] != null)
                                font.FontHeightInPoints = double.Parse(ct["fs"]?.ToString()!);
                            cell.CellStyle.SetFont(font);
                            //判断背景色
                            if (ct?["bg"] != null)
                            {
                                ICellStyle cellStyle1 = workbook.CreateCellStyle();
                                cellStyle1.CloneStyleFrom(cellStyle);
                                if (ct["bg"] != null)
                                {
                                    var rGb = HTMLColorTranslator.Translate(ct["bg"]?.ToString()!);
                                    var color = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    cellStyle1.FillPattern = FillPattern.SolidForeground;
                                    cellStyle1.FillForegroundColor = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(color.R, color.G, color.B).Indexed;
                                }
                                cell.CellStyle = cellStyle1;
                            }
                            else
                            {
                                cell.CellStyle = cellStyle;
                            }
                            //Cell.SetCellValue(ct["v"] != null ? ct["v"].ToString() : "");
                        }
                    }
                }
                sheet.ForceFormulaRecalculation = true;
            }
            //判断是否要设置列宽度
            if (columnlen != null)
            {
                foreach (var cols in columnlen)
                {
                    var p = cols as JProperty;
                    sheet.SetColumnWidth(int.Parse(p?.Name!), int.Parse(p?.Value.ToString()!) * 38);
                }
            }
            //判断是否要设置行高度
            if (rowlen != null)
            {
                foreach (var rows in rowlen)
                {
                    var p = rows as JProperty;
                    sheet.GetRow(int.Parse(p?.Name!)).HeightInPoints = float.Parse(p?.Value.ToString()!);
                }
            }
            //判断是否要加边框
            if (borderInfo != null)
            {
                for (int i = 0; i < borderInfo.Count(); i++)
                {
                    var bordervalue = borderInfo[i]?["value"];
                    if (bordervalue != null)
                    {
                        var rowindex = bordervalue["row_index"];
                        var colindex = bordervalue["col_index"];
                        var l = bordervalue["l"];
                        var r = bordervalue["r"];
                        var t = bordervalue["t"];
                        var b = bordervalue["b"];
                        if (rowindex != null)
                        {
                            IRow rows = sheet.GetRow(int.Parse(bordervalue["row_index"]?.ToString()!));
                            if (colindex != null)
                            {
                                ICell cell = rows.GetCell(int.Parse(bordervalue["col_index"]?.ToString()!));
                                if (b != null)
                                {
                                    cell.CellStyle.BorderBottom = ExcelHelper.GetBorderStyle(int.Parse(b["style"]?.ToString()!));
                                    var rGb = HTMLColorTranslator.Translate(b["color"]?.ToString()!);
                                    var bcolor = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    cell.CellStyle.BottomBorderColor = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(bcolor.R, bcolor.G, bcolor.B).Indexed;
                                }
                                else
                                {
                                    cell.CellStyle.BorderBottom = BorderStyle.None;
                                    cell.CellStyle.BottomBorderColor = HSSFColor.COLOR_NORMAL;
                                }
                                if (t != null)
                                {
                                    cell.CellStyle.BorderTop = ExcelHelper.GetBorderStyle(int.Parse(t["style"]?.ToString()!));
                                    var rGb = HTMLColorTranslator.Translate(t["color"]?.ToString()!);
                                    var tcolor = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    cell.CellStyle.TopBorderColor = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(tcolor.R, tcolor.G, tcolor.B).Indexed;
                                }
                                else
                                {
                                    cell.CellStyle.BorderBottom = BorderStyle.None;
                                    cell.CellStyle.BottomBorderColor = HSSFColor.COLOR_NORMAL;
                                }
                                if (l != null)
                                {
                                    cell.CellStyle.BorderLeft = ExcelHelper.GetBorderStyle(int.Parse(l["style"]?.ToString()!));
                                    var rGb = HTMLColorTranslator.Translate(l["color"]?.ToString()!);
                                    var lcolor = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    cell.CellStyle.LeftBorderColor = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(lcolor.R, lcolor.G, lcolor.B).Indexed;
                                }
                                else
                                {
                                    cell.CellStyle.BorderBottom = BorderStyle.None;
                                    cell.CellStyle.BottomBorderColor = HSSFColor.COLOR_NORMAL;
                                }
                                if (r != null)
                                {
                                    cell.CellStyle.BorderRight = ExcelHelper.GetBorderStyle(int.Parse(r["style"]?.ToString()!));
                                    var rGb = HTMLColorTranslator.Translate(r["color"]?.ToString()!);
                                    var rcolor = Color.FromArgb(rGb.R, rGb.G, rGb.B);
                                    cell.CellStyle.RightBorderColor = ((HSSFWorkbook)workbook).GetCustomPalette().FindSimilarColor(rcolor.R, rcolor.G, rcolor.B).Indexed;
                                }
                                else
                                {
                                    cell.CellStyle.BorderBottom = BorderStyle.None;
                                    cell.CellStyle.BottomBorderColor = HSSFColor.COLOR_NORMAL;
                                }
                            }
                        }
                    }
                }
            }
            //判断是否要合并单元格
            if (merge != null)
            {
                foreach (var imerge in merge)
                {
                    var firstmer = imerge.First();
                    int r = int.Parse(firstmer["r"]?.ToString()!);//主单元格的行号,开始行号
                    int rs = int.Parse(firstmer["rs"]?.ToString()!);//合并单元格占的行数,合并多少行
                    int c = int.Parse(firstmer["c"]?.ToString()!);//主单元格的列号,开始列号
                    int cs = int.Parse(firstmer["cs"]?.ToString()!);//合并单元格占的列数,合并多少列
                    CellRangeAddress region = new CellRangeAddress(r, r + rs - 1, c, c + cs - 1);
                    sheet.AddMergedRegion(region);
                }
            }
        }
        //string path = $@"{Directory.GetCurrentDirectory()}{@"\wwwroot\UploadFiles\" + DateTime.Now.ToString("yyyy-MM-dd") + @"\"}";
        //if (!Directory.Exists(path))
        //{
        //    Directory.CreateDirectory(path);
        //}
        //string fileName = $@"export_{DateTime.Now.ToString("hh_mm_ss")}.xls";
        //FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.OpenOrCreate);
        return workbook;
        //workbook.Write(stream);
        //stream.Seek(0, SeekOrigin.Begin);
        //workbook.Close();
        //stream.Close();
    }

    public static string GenerateExcelData(IWorkbook workbook,string strData)
    {
        List<JObject>? dowExcels = JsonConvert.DeserializeObject<List<JObject>>(strData);
          
        var item = dowExcels?[0];
            
        var cjarray = item?["data"];
        //var merge = item["config"]["merge"];
        //读取了模板内所有sheet内容
        ISheet sheet = workbook.GetSheet(item?["name"]?.ToString());
        //判断是否有值，并且赋予样式
        if (cjarray != null)
        {
            for (int i = 0; i < cjarray!.Count(); i++)
            {
                //判断行，存不存在，不存在创建
                IRow row = sheet.GetRow(i);
                if (row == null)
                {
                    row = sheet.CreateRow(i);
                }


                {
                    //没有合并单元格的走这边
                    //判断列，不存在创建
                    for (int columnIndex = 0; columnIndex < cjarray?[0]!.Count(); columnIndex++)
                    {
                        ICell cell = row.GetCell(columnIndex) ?? row.CreateCell(columnIndex);
                        try
                        {
                            if ((cjarray[i]?[columnIndex] as JObject)!=null&&((cjarray[i]?[columnIndex] as JObject)!).ContainsKey("m"))
                            {
                                var v = cjarray[i]?[columnIndex]?["m"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(v))
                                    cell.SetCellValue(v);
                            }
                                
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            //throw;
                        }
                    }
                }
            }
            sheet.ForceFormulaRecalculation = true;
        }
            
         
        string path = $@"{Directory.GetCurrentDirectory()}\wwwroot\UploadFiles\";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string fileName = $@"export_{DateTime.Now:yyyy-MM-dd-HHmmss}.xls";
        FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.OpenOrCreate);
        workbook.Write(stream);
        stream.Seek(0, SeekOrigin.Begin);
        workbook.Close();
        stream.Close();
        return fileName;
    }
}