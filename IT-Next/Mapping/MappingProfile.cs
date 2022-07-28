using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using IT_Next.Extensions;
using static IT_Next.Extensions.ExpressionExtensions;

namespace IT_Next.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, Product>();

        CreateMap<CategoryResource, Category>()
            .ForMember(c => c.Id, opt => opt.Ignore());
        CreateMap<SaveSubCategoryResource, SubCategory>()
            .ForMember(sc => sc.Id, opt => opt.Ignore());
        CreateMap<BrandResource, Brand>()
            .ForMember(b => b.Id, opt => opt.Ignore());
        CreateMap<ProductResource, Product>();
        CreateMap<SaveProductResource, Product>()
            .ForMember(p => p.Id, opt => opt.Ignore());

        CreateMap<CategoryQueryResource, Query<Category>>()
            .ConvertUsing((src, _) =>
            {
                var result = new Query<Category>(new Paging(src.PageNumber, src.PageSize));

                var sortBy = GenerateExpressionPropertySelector<Category>(src.SortBy);
                result.AddOrder(new Ordering<Category>(sortBy, src.IsSortAscending));

                if (!string.IsNullOrWhiteSpace(src.SearchQuery))
                    result.Conditions = GenerateOrConditionsPredicate<Category>(src.SearchQuery);

                return result;
            });

        CreateMap<SubCategoryQueryResource, Query<SubCategory>>()
            .ConvertUsing((src, _) =>
            {
                var result = new Query<SubCategory>(new Paging(src.PageNumber, src.PageSize));

                var sortBy = src.SortBy.ToLower() switch
                {
                    "categoryname" => c => c.Category.Name,
                    "categoryid" => c => c.Category.Id,
                    _ => GenerateExpressionPropertySelector<SubCategory>(src.SortBy)
                };
                result.AddOrder(new Ordering<SubCategory>(sortBy, src.IsSortAscending));

                if (string.IsNullOrWhiteSpace(src.SearchQuery))
                    return result;

                result.Conditions = GenerateOrConditionsPredicate<SubCategory>(src.SearchQuery);
                if (src.SearchByCategory)
                    result.Conditions = result.Conditions
                        .AppendOr(sc => sc.Category.Name.Contains(src.SearchQuery));

                return result;
            });

        CreateMap<BrandQueryResource, Query<Brand>>()
            .ConvertUsing((src, _) =>
            {
                var result = new Query<Brand>(new Paging(src.PageNumber, src.PageSize));

                var sortBy = GenerateExpressionPropertySelector<Brand>(src.SortBy);
                result.AddOrder(new Ordering<Brand>(sortBy, src.IsSortAscending));

                if (!string.IsNullOrWhiteSpace(src.SearchQuery))
                    result.Conditions = GenerateOrConditionsPredicate<Brand>(src.SearchQuery);

                return result;
            });

        CreateMap<ProductQueryResource, Query<Product>>()
            .ConvertUsing((src, _) =>
            {
                var result = new Query<Product>(new Paging(src.PageNumber, src.PageSize));

                var sortBy = src.SortBy.ToLower() switch
                {
                    "brandname" => p => p.Brand.Name,
                    "brandid" => p => p.Brand.Id,
                    "categoryname" => p => p.SubCategory.Category.Name,
                    "subcategoryname" => p => p.SubCategory.Name,
                    "subcategoryid" => p => p.SubCategory.Id,
                    _ => GenerateExpressionPropertySelector<Product>(src.SortBy)
                };
                result.AddOrder(new Ordering<Product>(sortBy, src.IsSortAscending));

                if (!string.IsNullOrWhiteSpace(src.SearchQuery))
                    result.Conditions = GenerateOrConditionsPredicate<Product>(src.SearchQuery)
                        .AppendOr(p => p.SubCategory.Name.Contains(src.SearchQuery))
                        .AppendOr(p => p.SubCategory.Category.Name.Contains(src.SearchQuery))
                        .AppendOr(p => p.Brand.Name.Contains(src.SearchQuery));

                return result;
            });

        CreateMap<Category, CategoryResource>();
        CreateMap<SubCategory, SubCategoryResource>()
            .ForMember(scr => scr.CategoryName, opt => opt.MapFrom(sc => sc.Category.Name))
            .ForMember(scr => scr.CategoryId, opt => opt.MapFrom(sc => sc.Category.Id));
        CreateMap<Brand, BrandResource>();
        CreateMap<Product, ProductResource>()
            .ForMember(pr => pr.SubCategoryId, opt => opt.MapFrom(p => p.SubCategory.Id))
            .ForMember(pr => pr.SubCategoryName, opt => opt.MapFrom(p => p.SubCategory.Name))
            .ForMember(pr => pr.CategoryName, opt => opt.MapFrom(p => p.SubCategory.Category.Name))
            .ForMember(pr => pr.BrandId, opt => opt.MapFrom(p => p.Brand.Id))
            .ForMember(pr => pr.BrandName, opt => opt.MapFrom(p => p.Brand.Name))
            .ForMember(pr => pr.ImagePath, opt => opt.MapFrom(p => string.Concat("/", p.ImagePath)));
    }
}