using TownManager.Domain.Enums;

namespace TownManager.Application.Dtos;

public record TrainOrderDto(
    Guid Id,
    TroopType TroopType,
    int Amount,
    int Completed,
    DateTime StartedAt,
    DateTime CompletesAt
);
