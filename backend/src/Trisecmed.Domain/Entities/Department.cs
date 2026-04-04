namespace Trisecmed.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }

    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
}
