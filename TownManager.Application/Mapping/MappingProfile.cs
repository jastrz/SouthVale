using AutoMapper;
using TownManager.Domain.Entities;
using TownManager.Application.Villages.Commands;
using TownManager.Domain.Enums;

namespace TownManager.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // CreateMap<IReadOnlyCollection<TroopEntry>, Troops>()
        //     .ConvertUsing(src => new Troops(
        //         src.Where(x => x.TroopType == TroopType.Swordsman)
        //             .Sum(x => x.Count),
        //         src.Where(x => x.TroopType == TroopType.Archer)
        //             .Sum(x => x.Count)));
    }
}