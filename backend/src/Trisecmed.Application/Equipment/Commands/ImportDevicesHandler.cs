using MediatR;
using Trisecmed.Application.Common;
using Trisecmed.Domain.Entities;
using Trisecmed.Domain.Enums;
using Trisecmed.Domain.Interfaces;

namespace Trisecmed.Application.Equipment.Commands;

public interface IExcelReader
{
    IReadOnlyList<Dictionary<string, string?>> ReadRows(Stream stream);
}

public class ImportDevicesHandler : IRequestHandler<ImportDevicesCommand, Result<ImportResult>>
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelReader _excelReader;

    public ImportDevicesHandler(IDeviceRepository deviceRepo, IUnitOfWork unitOfWork, IExcelReader excelReader)
    {
        _deviceRepo = deviceRepo;
        _unitOfWork = unitOfWork;
        _excelReader = excelReader;
    }

    public async Task<Result<ImportResult>> Handle(ImportDevicesCommand request, CancellationToken cancellationToken)
    {
        var rows = _excelReader.ReadRows(request.FileStream);

        var errors = new List<ImportError>();
        var imported = 0;
        var duplicates = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2; // Excel row (1-indexed + header)
            var row = rows[i];

            var name = GetValue(row, "Name", "Nazwa");
            var inventoryNumber = GetValue(row, "InventoryNumber", "NumerInwentarzowy", "Numer inwentarzowy");
            var manufacturer = GetValue(row, "Manufacturer", "Producent");
            var model = GetValue(row, "Model");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(new ImportError(rowNum, "Name", "Nazwa jest wymagana."));
                continue;
            }
            if (string.IsNullOrWhiteSpace(inventoryNumber))
            {
                errors.Add(new ImportError(rowNum, "InventoryNumber", "Numer inwentarzowy jest wymagany."));
                continue;
            }
            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                errors.Add(new ImportError(rowNum, "Manufacturer", "Producent jest wymagany."));
                continue;
            }
            if (string.IsNullOrWhiteSpace(model))
            {
                errors.Add(new ImportError(rowNum, "Model", "Model jest wymagany."));
                continue;
            }

            // Check duplicate
            var existing = await _deviceRepo.GetByInventoryNumberAsync(inventoryNumber, cancellationToken);
            if (existing is not null)
            {
                duplicates++;
                continue;
            }

            var device = new Device
            {
                Name = name,
                InventoryNumber = inventoryNumber,
                SerialNumber = GetValue(row, "SerialNumber", "NumerSeryjny", "Numer seryjny"),
                Manufacturer = manufacturer,
                Model = model,
                Status = DeviceStatus.Active,
                Notes = GetValue(row, "Notes", "Uwagi"),
                // Default IDs — admin should set proper category/department
                CategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                DepartmentId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedByUserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            };

            // Try to parse optional fields
            if (decimal.TryParse(GetValue(row, "PurchasePrice", "Cena", "Wartość"), out var price))
                device.PurchasePrice = price;

            await _deviceRepo.AddAsync(device, cancellationToken);
            imported++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ImportResult
        {
            Imported = imported,
            Duplicates = duplicates,
            Errors = errors,
        });
    }

    private static string? GetValue(Dictionary<string, string?> row, params string[] keys)
    {
        foreach (var key in keys)
        {
            // Try exact match first, then case-insensitive
            if (row.TryGetValue(key, out var val) && !string.IsNullOrWhiteSpace(val))
                return val.Trim();

            var match = row.FirstOrDefault(kv =>
                string.Equals(kv.Key?.Trim(), key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(match.Value))
                return match.Value.Trim();
        }
        return null;
    }
}
