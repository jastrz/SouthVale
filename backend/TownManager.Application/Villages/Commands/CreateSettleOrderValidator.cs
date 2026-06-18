using FluentValidation;
using TownManager.Domain.Entities;

namespace TownManager.Application.Villages.Commands;

public class CreateSettleOrderValidator : AbstractValidator<CreateSettleOrderCommand>
{
    public CreateSettleOrderValidator()
    {
        RuleFor(x => x.VillageId).NotEmpty();
        RuleFor(x => x.Target).NotNull();
        RuleFor(x => x.Target.X).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Target.Y).GreaterThanOrEqualTo(0);
    }
}
