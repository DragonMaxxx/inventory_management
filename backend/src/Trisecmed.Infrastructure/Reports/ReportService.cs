using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Trisecmed.Application.Reports;
using Trisecmed.Domain.Enums;
using Trisecmed.Infrastructure.Data;

namespace Trisecmed.Infrastructure.Reports;

public class ReportService : IReportService
{
    private readonly TrisecmedDbContext _db;

    public ReportService(TrisecmedDbContext db)
    {
        _db = db;
    }

    // ── Equipment PDF ──
    public async Task<byte[]> GenerateEquipmentReportPdfAsync(EquipmentReportRequest request, CancellationToken ct = default)
    {
        var devices = await GetFilteredDevices(request, ct);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Raport aparatury medycznej").FontSize(16).Bold().AlignCenter();
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Nazwa
                        c.RelativeColumn(1.5f); // Nr inw.
                        c.RelativeColumn(1.5f); // Producent
                        c.RelativeColumn(1); // Status
                        c.RelativeColumn(1.5f); // Oddział
                        c.RelativeColumn(1); // Kategoria
                        c.RelativeColumn(1); // Następny przegląd
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Nazwa").Bold();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Nr inwentarzowy").Bold();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Producent").Bold();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Status").Bold();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Oddział").Bold();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Kategoria").Bold();
                        h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Nast. przegląd").Bold();
                    });

                    foreach (var d in devices)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.Name);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.InventoryNumber);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.Manufacturer);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.Status.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.Department?.Name ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.Category?.Name ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(d.NextInspectionDate?.ToString("yyyy-MM-dd") ?? "—");
                    }
                });
                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Wygenerowano: ");
                    t.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    t.Span(" | Strona ");
                    t.CurrentPageNumber();
                    t.Span(" z ");
                    t.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    // ── Equipment XLSX ──
    public async Task<byte[]> GenerateEquipmentReportXlsxAsync(EquipmentReportRequest request, CancellationToken ct = default)
    {
        var devices = await GetFilteredDevices(request, ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Aparatura");

        var headers = new[] { "Nazwa", "Nr inwentarzowy", "Nr seryjny", "Producent", "Model", "Status", "Oddział", "Kategoria", "Data zakupu", "Cena", "Gwarancja do", "Nast. przegląd" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        for (int r = 0; r < devices.Count; r++)
        {
            var d = devices[r];
            ws.Cell(r + 2, 1).Value = d.Name;
            ws.Cell(r + 2, 2).Value = d.InventoryNumber;
            ws.Cell(r + 2, 3).Value = d.SerialNumber ?? "";
            ws.Cell(r + 2, 4).Value = d.Manufacturer;
            ws.Cell(r + 2, 5).Value = d.Model;
            ws.Cell(r + 2, 6).Value = d.Status.ToString();
            ws.Cell(r + 2, 7).Value = d.Department?.Name ?? "";
            ws.Cell(r + 2, 8).Value = d.Category?.Name ?? "";
            ws.Cell(r + 2, 9).Value = d.PurchaseDate?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(r + 2, 10).Value = d.PurchasePrice?.ToString("F2") ?? "";
            ws.Cell(r + 2, 11).Value = d.WarrantyExpires?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(r + 2, 12).Value = d.NextInspectionDate?.ToString("yyyy-MM-dd") ?? "";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Failures PDF ──
    public async Task<byte[]> GenerateFailuresReportPdfAsync(FailuresReportRequest request, CancellationToken ct = default)
    {
        var failures = await GetFilteredFailures(request, ct);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Raport awarii").FontSize(16).Bold().AlignCenter();
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Urządzenie
                        c.RelativeColumn(1); // Priorytet
                        c.RelativeColumn(1); // Status
                        c.RelativeColumn(2); // Opis
                        c.RelativeColumn(1.5f); // Oddział
                        c.RelativeColumn(1); // Koszt
                        c.RelativeColumn(1); // Zgłoszono
                        c.RelativeColumn(1); // Rozwiązano
                    });

                    table.Header(h =>
                    {
                        foreach (var hdr in new[] { "Urządzenie", "Priorytet", "Status", "Opis", "Oddział", "Koszt", "Zgłoszono", "Rozwiązano" })
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(hdr).Bold();
                    });

                    foreach (var f in failures)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.Device?.Name ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.Priority.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.Status.ToString());
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.Description.Length > 60 ? f.Description[..60] + "..." : f.Description);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.Department?.Name ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.RepairCost?.ToString("F2") ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.CreatedAt.ToString("yyyy-MM-dd"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(f.ResolvedAt?.ToString("yyyy-MM-dd") ?? "—");
                    }
                });

                var totalCost = failures.Where(f => f.RepairCost.HasValue).Sum(f => f.RepairCost!.Value);
                page.Footer().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"Łączny koszt napraw: {totalCost:F2} PLN | Liczba awarii: {failures.Count}");
                    row.RelativeItem().AlignRight().Text(t =>
                    {
                        t.Span("Strona ");
                        t.CurrentPageNumber();
                        t.Span(" z ");
                        t.TotalPages();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    // ── Failures XLSX ──
    public async Task<byte[]> GenerateFailuresReportXlsxAsync(FailuresReportRequest request, CancellationToken ct = default)
    {
        var failures = await GetFilteredFailures(request, ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Awarie");

        var headers = new[] { "Urządzenie", "Nr inwentarzowy", "Priorytet", "Status", "Opis", "Oddział", "Zgłosił", "Serwisant", "Koszt", "Zgłoszono", "Rozwiązano" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        for (int r = 0; r < failures.Count; r++)
        {
            var f = failures[r];
            ws.Cell(r + 2, 1).Value = f.Device?.Name ?? "";
            ws.Cell(r + 2, 2).Value = f.Device?.InventoryNumber ?? "";
            ws.Cell(r + 2, 3).Value = f.Priority.ToString();
            ws.Cell(r + 2, 4).Value = f.Status.ToString();
            ws.Cell(r + 2, 5).Value = f.Description;
            ws.Cell(r + 2, 6).Value = f.Department?.Name ?? "";
            ws.Cell(r + 2, 7).Value = f.ReportedByUser != null ? $"{f.ReportedByUser.FirstName} {f.ReportedByUser.LastName}" : "";
            ws.Cell(r + 2, 8).Value = f.ServiceProvider?.Name ?? "";
            ws.Cell(r + 2, 9).Value = f.RepairCost?.ToString("F2") ?? "";
            ws.Cell(r + 2, 10).Value = f.CreatedAt.ToString("yyyy-MM-dd");
            ws.Cell(r + 2, 11).Value = f.ResolvedAt?.ToString("yyyy-MM-dd") ?? "";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Inspections PDF ──
    public async Task<byte[]> GenerateInspectionsReportPdfAsync(InspectionsReportRequest request, CancellationToken ct = default)
    {
        var inspections = await GetFilteredInspections(request, ct);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.Header().Text("Raport przeglądów").FontSize(16).Bold().AlignCenter();
                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); // Urządzenie
                        c.RelativeColumn(1.5f); // Nr inw.
                        c.RelativeColumn(1); // Data
                        c.RelativeColumn(1); // Nast. data
                        c.RelativeColumn(1); // Wynik
                        c.RelativeColumn(1.5f); // Wykonał
                        c.RelativeColumn(2); // Uwagi
                    });

                    table.Header(h =>
                    {
                        foreach (var hdr in new[] { "Urządzenie", "Nr inwentarzowy", "Data", "Następny", "Wynik", "Wykonał", "Uwagi" })
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text(hdr).Bold();
                    });

                    foreach (var i in inspections)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.Device?.Name ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.Device?.InventoryNumber ?? "");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.InspectionDate.ToString("yyyy-MM-dd"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.NextInspectionDate?.ToString("yyyy-MM-dd") ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.Result ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.PerformedBy ?? "—");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(i.Notes ?? "");
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span($"Liczba przeglądów: {inspections.Count} | ");
                    t.Span("Strona ");
                    t.CurrentPageNumber();
                    t.Span(" z ");
                    t.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    // ── Inspections XLSX ──
    public async Task<byte[]> GenerateInspectionsReportXlsxAsync(InspectionsReportRequest request, CancellationToken ct = default)
    {
        var inspections = await GetFilteredInspections(request, ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Przeglądy");

        var headers = new[] { "Urządzenie", "Nr inwentarzowy", "Data przeglądu", "Następny przegląd", "Wynik", "Wykonał", "Uwagi" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(1, i + 1).Value = headers[i];
        ws.Row(1).Style.Font.Bold = true;

        for (int r = 0; r < inspections.Count; r++)
        {
            var ins = inspections[r];
            ws.Cell(r + 2, 1).Value = ins.Device?.Name ?? "";
            ws.Cell(r + 2, 2).Value = ins.Device?.InventoryNumber ?? "";
            ws.Cell(r + 2, 3).Value = ins.InspectionDate.ToString("yyyy-MM-dd");
            ws.Cell(r + 2, 4).Value = ins.NextInspectionDate?.ToString("yyyy-MM-dd") ?? "";
            ws.Cell(r + 2, 5).Value = ins.Result ?? "";
            ws.Cell(r + 2, 6).Value = ins.PerformedBy ?? "";
            ws.Cell(r + 2, 7).Value = ins.Notes ?? "";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ── GDPR Export ──
    public async Task<byte[]> ExportGdprDataAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null)
            throw new KeyNotFoundException("Użytkownik nie został znaleziony.");

        using var workbook = new XLWorkbook();

        // User data
        var wsUser = workbook.Worksheets.Add("Dane użytkownika");
        wsUser.Cell(1, 1).Value = "Pole";
        wsUser.Cell(1, 2).Value = "Wartość";
        wsUser.Row(1).Style.Font.Bold = true;

        var userData = new (string, string)[]
        {
            ("Id", user.Id.ToString()),
            ("Email", user.Email),
            ("Imię", user.FirstName),
            ("Nazwisko", user.LastName),
            ("Rola", user.Role.ToString()),
            ("Oddział", user.Department?.Name ?? "—"),
            ("Aktywny", user.IsActive ? "Tak" : "Nie"),
            ("Ostatnie logowanie", user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
            ("Konto utworzono", user.CreatedAt.ToString("yyyy-MM-dd HH:mm")),
        };
        for (int i = 0; i < userData.Length; i++)
        {
            wsUser.Cell(i + 2, 1).Value = userData[i].Item1;
            wsUser.Cell(i + 2, 2).Value = userData[i].Item2;
        }

        // Audit logs
        var auditLogs = await _db.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(1000)
            .ToListAsync(ct);

        var wsAudit = workbook.Worksheets.Add("Log audytu");
        var auditHeaders = new[] { "Data", "Akcja", "Typ encji", "ID encji", "IP" };
        for (int i = 0; i < auditHeaders.Length; i++)
            wsAudit.Cell(1, i + 1).Value = auditHeaders[i];
        wsAudit.Row(1).Style.Font.Bold = true;

        for (int r = 0; r < auditLogs.Count; r++)
        {
            var log = auditLogs[r];
            wsAudit.Cell(r + 2, 1).Value = log.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            wsAudit.Cell(r + 2, 2).Value = log.Action.ToString();
            wsAudit.Cell(r + 2, 3).Value = log.EntityType;
            wsAudit.Cell(r + 2, 4).Value = log.EntityId.ToString();
            wsAudit.Cell(r + 2, 5).Value = log.IpAddress ?? "";
        }

        // Reported failures
        var failures = await _db.Failures
            .Include(f => f.Device)
            .Where(f => f.ReportedByUserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

        var wsFail = workbook.Worksheets.Add("Zgłoszone awarie");
        var failHeaders = new[] { "Data", "Urządzenie", "Opis", "Status", "Priorytet" };
        for (int i = 0; i < failHeaders.Length; i++)
            wsFail.Cell(1, i + 1).Value = failHeaders[i];
        wsFail.Row(1).Style.Font.Bold = true;

        for (int r = 0; r < failures.Count; r++)
        {
            var f = failures[r];
            wsFail.Cell(r + 2, 1).Value = f.CreatedAt.ToString("yyyy-MM-dd");
            wsFail.Cell(r + 2, 2).Value = f.Device?.Name ?? "";
            wsFail.Cell(r + 2, 3).Value = f.Description;
            wsFail.Cell(r + 2, 4).Value = f.Status.ToString();
            wsFail.Cell(r + 2, 5).Value = f.Priority.ToString();
        }

        foreach (var ws in workbook.Worksheets)
            ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    // ── Private helpers ──
    private async Task<List<Domain.Entities.Device>> GetFilteredDevices(EquipmentReportRequest request, CancellationToken ct)
    {
        var query = _db.Devices
            .Include(d => d.Category)
            .Include(d => d.Department)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            query = query.Where(d => d.DepartmentId == request.DepartmentId.Value);
        if (request.CategoryId.HasValue)
            query = query.Where(d => d.CategoryId == request.CategoryId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<DeviceStatus>(request.Status, true, out var status))
            query = query.Where(d => d.Status == status);

        return await query.OrderBy(d => d.Name).ToListAsync(ct);
    }

    private async Task<List<Domain.Entities.Failure>> GetFilteredFailures(FailuresReportRequest request, CancellationToken ct)
    {
        var query = _db.Failures
            .Include(f => f.Device)
            .Include(f => f.Department)
            .Include(f => f.ReportedByUser)
            .Include(f => f.ServiceProvider)
            .AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(f => f.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(f => f.CreatedAt <= request.DateTo.Value);
        if (request.DepartmentId.HasValue)
            query = query.Where(f => f.DepartmentId == request.DepartmentId.Value);

        return await query.OrderByDescending(f => f.CreatedAt).ToListAsync(ct);
    }

    private async Task<List<Domain.Entities.Inspection>> GetFilteredInspections(InspectionsReportRequest request, CancellationToken ct)
    {
        var query = _db.Inspections
            .Include(i => i.Device)
            .AsQueryable();

        if (request.DateFrom.HasValue)
        {
            var from = DateOnly.FromDateTime(request.DateFrom.Value);
            query = query.Where(i => i.InspectionDate >= from);
        }
        if (request.DateTo.HasValue)
        {
            var to = DateOnly.FromDateTime(request.DateTo.Value);
            query = query.Where(i => i.InspectionDate <= to);
        }
        if (request.DepartmentId.HasValue)
            query = query.Where(i => i.Device.DepartmentId == request.DepartmentId.Value);

        return await query.OrderByDescending(i => i.InspectionDate).ToListAsync(ct);
    }
}
