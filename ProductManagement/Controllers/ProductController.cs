using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;

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
        public async Task<List<Product>> GetProducts(string? searchTerm,
                                                     string? sortColumn,
                                                     string? sortOrder)
        {
            IQueryable<Product> productsQuery = _productDbContext.Products;
            
            if(!string.IsNullOrWhiteSpace(searchTerm))
            {
                productsQuery = productsQuery.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.Description.Contains(searchTerm));
            }

            Expression<Func<Product, object>> keySelector = sortColumn?.ToLower() switch
            {
                "name" => product => product.ProductName,
                "price" => product => product.Price,
                "creation_date" => product => product.CreationDate,
                "is_available" => product => product.IsAvailable,
                _ => product => product.Id
            };

            if (sortOrder?.ToLower() == "desc")
            {
                productsQuery = productsQuery.OrderByDescending(keySelector);
            }
            else
            {
                productsQuery = productsQuery.OrderBy(keySelector);
            }

            var products = await productsQuery
                .Select(p => new Product(
                    p.Id,
                    p.ProductName,
                    p.Description,
                    p.Price,
                    p.IsAvailable,
                    p.UserId,
                    p.CreationDate
                    ))
                .ToListAsync();

            return products;
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
            product.UserId = Int32.Parse(User.FindFirstValue("Id"));
            await _productDbContext.Products.AddAsync(product);
            await _productDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Update(Product product)
        {
            if (User.FindFirstValue("Id") != product.UserId.ToString())
            {
                return BadRequest();
            }
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

            if (User.FindFirstValue("Id") != product.UserId.ToString())
            {
                return BadRequest();
            }

            _productDbContext.Products.Remove(product);
            await _productDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}

