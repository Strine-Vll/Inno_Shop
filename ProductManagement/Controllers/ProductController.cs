using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Models;

namespace ProductManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductDbContext _productDbContext;

        public ProductController(ProductDbContext dbContext)
        {
            _productDbContext = dbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProduct()
        {
            return _productDbContext.Products;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _productDbContext.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPost]
        public async Task<ActionResult> Create(Product product)
        {
            await _productDbContext.Products.AddAsync(product);
            await _productDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Update(Product product)
        {
            if (!_productDbContext.Products.Any(existingProduct => existingProduct.Id == product.Id))
            {
                return NotFound();
            }

            _productDbContext.Products.Update(product);
            await _productDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _productDbContext.Products.FindAsync(id);

            if (product is null)
            {
                return NotFound();
            }

            _productDbContext.Products.Remove(product);
            await _productDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}

