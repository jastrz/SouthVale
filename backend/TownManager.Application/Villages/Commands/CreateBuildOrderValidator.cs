using FluentValidation;

namespace TownManager.Application.Villages.Commands;

public class CreateBuildOrderValidator : AbstractValidator<CreateBuildOrderCommand>
{
    public CreateBuildOrderValidator()
    {
        RuleFor(x => x.VillageId).NotEmpty();
        RuleFor(x => x.BuildingType).IsInEnum();
    }
}