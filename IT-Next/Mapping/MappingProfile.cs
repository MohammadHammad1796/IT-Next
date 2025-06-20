using AutoMapper;
using IT_Next.Controllers.APIs.Resources;
using IT_Next.Controllers.UIs.ViewModels;
using IT_Next.Core.Entities;
using IT_Next.Core.Helpers;
using static IT_Next.Core.Extensions.ExpressionExtensions;

namespace IT_Next.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CoreToCore();

        AppToCore();

        CoreToApp();
    }

    private void CoreToCore()
    {
        CreateMap<Product, Product>();
    }

    private void CoreToApp()
    {
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
            .ForMember(pr => pr.ImagePath, opt => opt.MapFrom(p => p.ImagePath));
        CreateMap<Product, ProductInSubCategoryProductsListResource>()
            .ForMember(res => res.Brand, opt => opt.MapFrom(p => p.Brand.Name))
            .ForMember(res => res.ImagePath, opt => opt.MapFrom(p => p.ImagePath))
            .ForMember(res => res.OldPrice, opt => opt.MapFrom(p => p.Price))
            .ForMember(res => res.NewPrice, opt => opt.MapFrom(p => p.Price * (1 - p.Discount)));

        CreateMap<IEnumerable<Product>, HomeViewModel>()
            .ConvertUsing((src, _) =>
            {
                var viewModel = new HomeViewModel();
                var productsInList = new List<ProductInListViewModel>();
                foreach (var product in src)
                {
                    var productInList = new ProductInListViewModel
                    {
                        Name = product.Name,
                        Brand = product.Brand.Name,
                        ImagePath = product.ImagePath,
                        OldPrice = product.Price,
                        CategoryName = product.SubCategory.Category.Name,
                        SubCategoryName = product.SubCategory.Name
                    };
                    if (product.Discount > 0)
                        productInList.NewPrice = product.Price * (1 - product.Discount);

                    productsInList.Add(productInList);
                }

                viewModel.LatestProducts = productsInList;
                return viewModel;
            });
        CreateMap<Product, ProductDetailsViewModel>()
            .ForMember(pr => pr.SubCategoryName, opt => opt.MapFrom(p => p.SubCategory.Name))
            .ForMember(pr => pr.CategoryName, opt => opt.MapFrom(p => p.SubCategory.Category.Name))
            .ForMember(pr => pr.BrandName, opt => opt.MapFrom(p => p.Brand.Name))
            .ForMember(pr => pr.ImagePath, opt => opt.MapFrom(p => p.ImagePath));

        CreateMap<IEnumerable<SubCategory>, ProductsNavigationByCategoriesViewModel>()
            .ConvertUsing((src, _) =>
            {
                var groups = src.GroupBy(s => s.Category);
                var navigationViewModel = new ProductsNavigationByCategoriesViewModel();
                foreach (var group in groups)
                {
                    var categoryNavigation = new CategoryNavigationViewModel(group.Key.Name);

                    foreach (var subCategory in group.Key.SubCategories)
                    {
                        var subCategoryNavigationViewModel = new SubCategoryNavigationViewModel(subCategory.Name);
                        categoryNavigation.AddSubCategoryNavigation(subCategoryNavigationViewModel);
                    }

                    navigationViewModel.AddCategoryNavigation(categoryNavigation);
                }

                return navigationViewModel;
            });
    }

    private void AppToCore()
    {
        CreateMap<CategoryResource, Category>()
            .ForMember(c => c.Id, opt => opt.Ignore());
        CreateMap<SaveSubCategoryResource, SubCategory>()
            .ForMember(sc => sc.Id, opt => opt.Ignore());
        CreateMap<BrandResource, Brand>()
            .ForMember(b => b.Id, opt => opt.Ignore());
        CreateMap<ProductResource, Product>();
        CreateMap<SaveProductResource, Product>()
            .ForMember(p => p.Id, opt => opt.Ignore());
        CreateMap<SendContactMessageResource, ContactMessage>();

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

        CreateMap<SubCategoryProductsQueryResource, Query<Product>>()
            .ConvertUsing((src, result) =>
            {
                result.Paging = new Paging(src.PageNumber, src.PageSize);

                var sortBy = src.SortBy.ToLower() switch
                {
                    "brand" => p => p.Brand.Name,
                    "price" => p => p.Price * (1 - p.Discount),
                    _ => GenerateExpressionPropertySelector<Product>(src.SortBy)
                };
                result.AddOrder(new Ordering<Product>(sortBy, src.IsSortAscending));

                result.AddIncludeProperty(p => p.Brand);

                if (!string.IsNullOrWhiteSpace(src.SearchQuery))
                    result.Conditions = result.Conditions!.AppendAnd(p => p.Name.Contains(src.SearchQuery));

                if (src.MinimumPrice.HasValue && src.MaximumPrice.HasValue)
                {
                    if (src.MaximumPrice.Value != src.MinimumPrice.Value)
                        result.Conditions = result.Conditions!.AppendAnd(p => (p.Price * (1 - p.Discount)) >= src.MinimumPrice!.Value
                     && (p.Price * (1 - p.Discount)) <= src.MaximumPrice!.Value);
                    else
                        result.Conditions = result.Conditions!.AppendAnd(p => (p.Price * (1 - p.Discount)) == src.MinimumPrice!.Value);
                }
                else if (src.MinimumPrice.HasValue)
                    result.Conditions = result.Conditions!.AppendAnd(p => (p.Price * (1 - p.Discount)) >= src.MinimumPrice!.Value);
                else if (src.MaximumPrice.HasValue)
                    result.Conditions = result.Conditions!.AppendAnd(p => (p.Price * (1 - p.Discount)) <= src.MaximumPrice!.Value);

                if (!src.BrandsIds.Any())
                    return result;

                result.Conditions = result.Conditions!.AppendAnd(p => src.BrandsIds.Contains(p.BrandId));

                return result;
            });

        CreateMap<ContactMessageQueryResource, Query<ContactMessage>>()
            .ConvertUsing((src, _) =>
            {
                var result = new Query<ContactMessage>(new Paging(src.PageNumber, src.PageSize));

                var sortBy = GenerateExpressionPropertySelector<ContactMessage>(src.SortBy);
                result.AddOrder(new Ordering<ContactMessage>(sortBy, src.IsSortAscending));

                if (!string.IsNullOrWhiteSpace(src.SearchQuery))
                    result.Conditions = GenerateOrConditionsPredicate<ContactMessage>(src.SearchQuery);

                return result;
            });
    }
}