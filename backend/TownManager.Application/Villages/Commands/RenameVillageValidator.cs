using FluentValidation;

namespace TownManager.Application.Villages.Commands;

public class RenameVillageValidator : AbstractValidator<RenameVillageCommand>
{
    public RenameVillageValidator()
    {
        RuleFor(x => x.NewName).NotEmpty().MaximumLength(30);
    }
}
