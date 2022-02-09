using Microsoft.AspNetCore.Mvc;
using WebStore.Interfaces.Services;

namespace WebStore.Components;

public class CartViewComponent : ViewComponent
{
    private readonly ICartStore _CartStore;

    public CartViewComponent(ICartStore CartStore)
    {
        _CartStore = CartStore;
    }

    public IViewComponentResult Invoke()
    {
        ViewBag.Count = _CartStore.Cart.ItemsCount;
        return View();
    }
}
