namespace Trisecmed.Domain.Entities;

public class MedicalDevice
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string SerialNumber { get; set; } = null!;
}