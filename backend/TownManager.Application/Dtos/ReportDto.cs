using TownManager.Domain.Enums;

namespace TownManager.Application.Dtos;

public record ReportDto(
    Guid Id,
    ReportType Type,
    string Title,
    string Body,
    bool IsRead,
    DateTime CreatedAt
);
