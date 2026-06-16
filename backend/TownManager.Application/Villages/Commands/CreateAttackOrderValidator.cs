using FluentValidation;

namespace TownManager.Application.Villages.Commands;

public class CreateAttackOrderValidator : AbstractValidator<CreateAttackOrderCommand>
{
    public CreateAttackOrderValidator()
    {
        RuleFor(x => x.Troops)
            .NotEmpty()
            .WithMessage("No troops specified for attack.");

        RuleForEach(x => x.Troops)
            .ChildRules(entry => entry
                .RuleFor(e => e.Count).GreaterThan(0).WithMessage("Troop count must be positive."));
    }
}
