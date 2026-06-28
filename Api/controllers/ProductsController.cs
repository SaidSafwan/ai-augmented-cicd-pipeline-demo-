using Microsoft.AspNetCore.Mvc;

namespace Api.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // Hardcoded in-memory product catalog.
        private static readonly List<Product> Products = new()
        {
            new Product(1, "Keyboard", 49.99m),
            new Product(2, "Mouse", 24.99m),
            new Product(3, "Monitor", 199.99m),
            new Product(4, "Webcam", 79.99m),
            new Product(5, "Headset", 89.99m)
        };

        // Memory leak lever: static so created orders are never garbage collected.
        private static readonly List<Order> Orders = new();

        // GET /api/products
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(Products);
        }

        // GET /api/products/{id}?slow=true&crash=true
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] bool slow = false, [FromQuery] bool crash = false)
        {
            if (crash)
            {
                // Intentionally uncaught so ASP.NET returns 500 and App Insights
                // records a failed request.
                throw new InvalidOperationException("Simulated downstream failure for demo");
            }

            if (slow)
            {
                // Simulate a slow downstream query.
                await Task.Delay(5000);
            }

            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST /api/orders
        [HttpPost("/api/orders")]
        public IActionResult CreateOrder([FromBody] OrderRequest request)
        {
            var order = new Order(Orders.Count + 1, request.ProductId, request.Quantity);
            Orders.Add(order);
            return StatusCode(StatusCodes.Status201Created, order);
        }
    }

    public record Product(int Id, string Name, decimal Price);

    public record Order(int Id, int ProductId, int Quantity);

    public record OrderRequest(int ProductId, int Quantity);
}
