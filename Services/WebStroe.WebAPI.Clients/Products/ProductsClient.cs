using System.Net.Http.Json;
using WebStore.Domain;
using WebStore.Domain.DTO;
using WebStore.Domain.Entities;
using WebStore.Interfaces.Services;
using WebStroe.WebAPI.Clients.Base;

namespace WebStroe.WebAPI.Clients.Products;

public class ProductsClient : BaseClient, IProductData
{
    public ProductsClient(HttpClient Client) : base(Client, "api/products")
    {
    }

    public Product CreateProduct(string Name, int Order, decimal Price, string ImageUrl, string Section, string? Brand = null)
    {
        var response = Post($"{Address}/new", new CreateProductDTO
        {
            Name = Name,
            Order = Order,
            Price = Price,
            ImageUrl = ImageUrl,
            Section = Section,
            Brand = Brand,
        });

        var product = response.Content.ReadFromJsonAsync<Product>().Result;
        return product!;
    }

    public Brand? GetBrandById(int Id)
    {
        var brand = Get<Brand>($"{Address}/brands/{Id}");
        return brand;
    }

    public IEnumerable<Brand> GetBrands()
    {
        var brands = Get<IEnumerable<Brand>>($"{Address}/brands");
        return brands!;
    }

    public Product? GetProductById(int Id)
    {
        var product = Get<Product>($"{Address}/{Id}");
        return product;
    }

    public IEnumerable<Product> GetProducts(ProductFilter? Filter = null)
    {
        var response = Post(Address, Filter ?? new());
        var products = response.Content.ReadFromJsonAsync<IEnumerable<Product>>().Result;
        return products!;
    }

    public Section? GetSectionById(int Id)
    {
        var section = Get<Section>($"{Address}/sections/{Id}");
        return section;
    }

    public IEnumerable<Section> GetSections()
    {
        var sections = Get<IEnumerable<Section>>($"{Address}/sections");
        return sections!;
    }
}
