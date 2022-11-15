namespace IT_Next.Controllers.UIs.ViewModels;

public class CategoryNavigationViewModel
{
    public string Name { get; set; }

    public ICollection<SubCategoryNavigationViewModel> SubCategories { get; set; }

    public CategoryNavigationViewModel(string name)
    {
        Name = name;
        SubCategories = new List<SubCategoryNavigationViewModel>();
    }

    public void AddSubCategoryNavigation(SubCategoryNavigationViewModel viewModel)
    {
        SubCategories.Add(viewModel);
    }
}