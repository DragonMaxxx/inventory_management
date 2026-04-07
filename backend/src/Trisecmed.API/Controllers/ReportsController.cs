using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trisecmed.Application.Reports;

namespace Trisecmed.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Raport aparatury (PDF/XLSX)</summary>
    [HttpPost("equipment")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> EquipmentReport([FromBody] EquipmentReportRequest request)
    {
        if (string.Equals(request.Format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await _reportService.GenerateEquipmentReportXlsxAsync(request);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "raport-aparatura.xlsx");
        }

        var pdf = await _reportService.GenerateEquipmentReportPdfAsync(request);
        return File(pdf, "application/pdf", "raport-aparatura.pdf");
    }

    /// <summary>Raport awarii z kosztami (PDF/XLSX)</summary>
    [HttpPost("failures")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> FailuresReport([FromBody] FailuresReportRequest request)
    {
        if (string.Equals(request.Format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await _reportService.GenerateFailuresReportXlsxAsync(request);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "raport-awarie.xlsx");
        }

        var pdf = await _reportService.GenerateFailuresReportPdfAsync(request);
        return File(pdf, "application/pdf", "raport-awarie.pdf");
    }

    /// <summary>Raport przeglądów (PDF/XLSX)</summary>
    [HttpPost("inspections")]
    [Authorize(Roles = "EquipmentWorker,EquipmentManager")]
    public async Task<IActionResult> InspectionsReport([FromBody] InspectionsReportRequest request)
    {
        if (string.Equals(request.Format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            var bytes = await _reportService.GenerateInspectionsReportXlsxAsync(request);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "raport-przeglady.xlsx");
        }

        var pdf = await _reportService.GenerateInspectionsReportPdfAsync(request);
        return File(pdf, "application/pdf", "raport-przeglady.pdf");
    }

    /// <summary>Eksport danych użytkownika RODO (XLSX)</summary>
    [HttpGet("export/gdpr/{userId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GdprExport(Guid userId)
    {
        try
        {
            var bytes = await _reportService.ExportGdprDataAsync(userId);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"gdpr-export-{userId}.xlsx");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
