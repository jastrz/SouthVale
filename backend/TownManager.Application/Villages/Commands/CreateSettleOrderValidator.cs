using FluentValidation;

namespace TownManager.Application.Villages.Commands;

public class CreateSettleOrderValidator : AbstractValidator<CreateSettleOrderCommand>
{
    public CreateSettleOrderValidator()
    {
        RuleFor(x => x.VillageId).NotEmpty();
        RuleFor(x => x.TargetX).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TargetY).GreaterThanOrEqualTo(0);
    }
}
