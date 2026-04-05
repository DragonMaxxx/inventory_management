using ClosedXML.Excel;
using Trisecmed.Application.Equipment.Commands;

namespace Trisecmed.Infrastructure.Services;

public class ClosedXmlExcelReader : IExcelReader
{
    public IReadOnlyList<Dictionary<string, string?>> ReadRows(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        var headerRow = worksheet.Row(1);
        var headers = new List<string>();
        var lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;

        for (var col = 1; col <= lastCol; col++)
        {
            var header = headerRow.Cell(col).GetString()?.Trim() ?? $"Column{col}";
            headers.Add(header);
        }

        var rows = new List<Dictionary<string, string?>>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var row = 2; row <= lastRow; row++)
        {
            var wsRow = worksheet.Row(row);
            var dict = new Dictionary<string, string?>();
            var hasData = false;

            for (var col = 0; col < headers.Count; col++)
            {
                var value = wsRow.Cell(col + 1).GetString()?.Trim();
                dict[headers[col]] = string.IsNullOrEmpty(value) ? null : value;
                if (!string.IsNullOrEmpty(value)) hasData = true;
            }

            if (hasData) rows.Add(dict);
        }

        return rows;
    }
}
