using FluentValidation;
using TownManager.Domain.Enums;

namespace TownManager.Application.Villages.Commands;

public class TradeResourcesValidator : AbstractValidator<TradeResourcesCommand>
{
    public TradeResourcesValidator()
    {
        RuleFor(x => x.GiveType).IsInEnum();
        RuleFor(x => x.ReceiveType).IsInEnum();
        RuleFor(x => x.GiveAmount).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.GiveType != x.ReceiveType)
            .WithMessage("Cannot trade a resource for itself.");
    }
}
