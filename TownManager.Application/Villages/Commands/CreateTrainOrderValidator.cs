using FluentValidation;

namespace TownManager.Application.Villages.Commands;

public class CreateTrainOrderValidator : AbstractValidator<CreateTrainOrderCommand>
{
    public CreateTrainOrderValidator() 
    {
        RuleFor(x => x.Orders).NotEmpty().WithMessage("No orders provided.");
        
        RuleForEach(x => x.Orders)
            .ChildRules(order => order
                .RuleFor(o => o.Count).GreaterThan(0).WithMessage("Count must be greater than 0."));
        
        RuleFor(x => x.Orders)
            .Must(orders => orders.GroupBy(o => o.TroopType).All(g => g.Count() == 1))
            .WithMessage("Duplicate troop types in order.");
    }
}