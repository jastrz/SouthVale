using FluentValidation;

namespace TownManager.Application.Villages.Commands;

public class CreateTransportOrderValidator : AbstractValidator<CreateTransportOrderCommand>
{
    public CreateTransportOrderValidator()
    {
        RuleFor(x => x.TargetVillageId).NotEmpty();

        RuleFor(x => new { x.Troops, x.Resources })
            .Must(x => x.Troops.Count > 0 || x.Resources.Wood > 0 || x.Resources.Clay > 0 || x.Resources.Iron > 0 || x.Resources.Beer > 0)
            .WithMessage("Send at least some troops or resources.");

        RuleForEach(x => x.Troops)
            .ChildRules(entry => entry
                .RuleFor(e => e.Count).GreaterThan(0).WithMessage("Troop count must be positive."));

        RuleFor(x => x.Resources.Wood).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Resources.Clay).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Resources.Iron).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Resources.Beer).GreaterThanOrEqualTo(0);
    }
}
