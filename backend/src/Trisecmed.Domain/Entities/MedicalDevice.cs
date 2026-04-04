using System;

namespace Trisecmed.Domain.Entities
{
    public class MedicalDevice
    {
        public Guid Id { get; set; }       // używamy GUID
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
    }
}