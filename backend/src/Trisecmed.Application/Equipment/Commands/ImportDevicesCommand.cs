using MediatR;
using Trisecmed.Application.Common;

namespace Trisecmed.Application.Equipment.Commands;

public record ImportDevicesCommand(Stream FileStream, string FileName) : IRequest<Result<ImportResult>>;

public record ImportResult
{
    public int Imported { get; init; }
    public int Duplicates { get; init; }
    public IReadOnlyList<ImportError> Errors { get; init; } = [];
}

public record ImportError(int Row, string Field, string Message);
