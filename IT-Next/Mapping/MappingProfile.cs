using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;

namespace IT_Next.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CategoryResource, Category>()
            .ForMember(c => c.Id, opt => opt.Ignore());
        CreateMap<CategoryQueryResource, Query<Category>>()
            .ConvertUsing((src, _) =>
            {
                var result = new Query<Category>();
                result.AddOrder(new Ordering<Category>(c => c.Name, src.IsSortAscending));
                result.Paging = new Paging(src.PageNumber, src.PageSize);
                if (!string.IsNullOrWhiteSpace(src.NameQuery))
                    result.Conditions = c => c.Name.Contains(src.NameQuery);

                return result;
            });

        CreateMap<Category, CategoryResource>();
    }
}