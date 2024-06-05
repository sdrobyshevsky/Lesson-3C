// Разработка web-приложения на C# (семинары)
// Урок 3. GraphQL и микросервисная архитектура
// Добавьте отдельный сервис позволяющий хранить информацию о товарах на складе/магазине. 
// Реализуйте к нему доступ посредством API и GraphQL.
// Реализуйте API-Gateway для API сервиса склада и API-сервиса из второй лекции.

// Для начала создадим отдельный сервис для хранения информации о товарах на складе/магазине
public class ProductService
{
    private List<Product> _products = new List<Product>();

    public Product GetProductById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public List<Product> GetProducts()
    {
        return _products;
    }

    public void AddProduct(Product product)
    {
        _products.Add(product);
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // Другие свойства товара
}

// Далее создадим API и GraphQL для доступа к сервису ProductService

// Реализация API контроллера для ProductService
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return _productService.GetProducts();
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return product;
    }
}

// Реализация GraphQL для ProductService
public class ProductSchema : Schema
{
    public ProductSchema(IDependencyResolver resolver) : base(resolver)
    {
        Query = resolver.Resolve<ProductQuery>();
    }
}

public class ProductQuery : ObjectGraphType
{
    public ProductQuery(ProductService productService)
    {
        Field<ListGraphType<ProductType>>(
            "products",
            resolve: context => productService.GetProducts()
        );

        Field<ProductType>(
            "product",
            arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "id" }),
            resolve: context =>
            {
                var id = context.GetArgument<int>("id");
                return productService.GetProductById(id);
            }
        );
    }
}

public class ProductType : ObjectGraphType<Product>
{
    public ProductType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
        Field(x => x.Price);
        // Добавляем остальные поля товара
    }
}

// Наконец, реализуем API Gateway для ProductService и сервиса из второй лекции

// Взаимодействие с другими микросервисами через HttpClient или gRPC
public class ApiGateway
{
    private HttpClient _httpClient;

    public ApiGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetProductInfoFromWarehouse(int productId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"https://warehouse-service/api/products/{productId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return null;
    }

    // Добавляем другие методы для взаимодействия с другими микросервисами
}

// Этот код демонстрирует создание отдельного сервиса для хранения информации о товарах на складе/магазине,
// реализацию доступа к этому сервису через API и GraphQL,
// а также использование API Gateway для взаимодействия с другими микросервисами.