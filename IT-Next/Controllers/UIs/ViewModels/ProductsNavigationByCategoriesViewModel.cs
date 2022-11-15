namespace IT_Next.Controllers.UIs.ViewModels;

public class ProductsNavigationByCategoriesViewModel
{
    public ICollection<CategoryNavigationViewModel> CategoriesNavigation { get; set; }

    public ProductsNavigationByCategoriesViewModel()
    {
        CategoriesNavigation = new List<CategoryNavigationViewModel>();
    }

    public void AddCategoryNavigation(CategoryNavigationViewModel viewModel)
    {
        CategoriesNavigation.Add(viewModel);
    }
}