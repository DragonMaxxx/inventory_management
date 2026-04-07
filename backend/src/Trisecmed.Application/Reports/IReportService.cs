namespace Trisecmed.Application.Reports;

public interface IReportService
{
    Task<byte[]> GenerateEquipmentReportPdfAsync(EquipmentReportRequest request, CancellationToken ct = default);
    Task<byte[]> GenerateEquipmentReportXlsxAsync(EquipmentReportRequest request, CancellationToken ct = default);
    Task<byte[]> GenerateFailuresReportPdfAsync(FailuresReportRequest request, CancellationToken ct = default);
    Task<byte[]> GenerateFailuresReportXlsxAsync(FailuresReportRequest request, CancellationToken ct = default);
    Task<byte[]> GenerateInspectionsReportPdfAsync(InspectionsReportRequest request, CancellationToken ct = default);
    Task<byte[]> GenerateInspectionsReportXlsxAsync(InspectionsReportRequest request, CancellationToken ct = default);
    Task<byte[]> ExportGdprDataAsync(Guid userId, CancellationToken ct = default);
}

public record EquipmentReportRequest
{
    public Guid? DepartmentId { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Status { get; init; }
    public string Format { get; init; } = "pdf";
}

public record FailuresReportRequest
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public Guid? DepartmentId { get; init; }
    public string Format { get; init; } = "pdf";
}

public record InspectionsReportRequest
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public Guid? DepartmentId { get; init; }
    public string Format { get; init; } = "pdf";
}
