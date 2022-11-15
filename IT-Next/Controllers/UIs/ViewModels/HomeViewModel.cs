namespace IT_Next.Controllers.UIs.ViewModels;

public class HomeViewModel
{
    public ICollection<ProductInListViewModel> LatestProducts { get; set; }

    public HomeViewModel()
    {
        LatestProducts = new List<ProductInListViewModel>();        
    }
}