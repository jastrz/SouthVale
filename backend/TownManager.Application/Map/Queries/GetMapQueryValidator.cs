using FluentValidation;

namespace TownManager.Application.Map.Queries;

public class GetMapQueryValidator : AbstractValidator<GetMapQuery>
{
    public GetMapQueryValidator()
    {
        RuleFor(x => x.cords).NotNull();
        RuleFor(x => x.radius).GreaterThan(0).LessThanOrEqualTo(200);
    }
}
