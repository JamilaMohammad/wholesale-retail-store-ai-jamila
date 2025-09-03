using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CommerceHub.API.Data;
using CommerceHub.API.DTOs;
using CommerceHub.API.Models;

namespace CommerceHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly CommerceHubContext _context;

        public ProductsController(CommerceHubContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] string? search = null,
            [FromQuery] string? category = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (!string.IsNullOrEmpty(category) && category != "all")
            {
                query = query.Where(p => p.Category == category);
            }

            var products = await query
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    WholesalePrice = p.WholesalePrice,
                    RetailPrice = p.RetailPrice,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    InStock = p.InStock,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    WholesalePrice = p.WholesalePrice,
                    RetailPrice = p.RetailPrice,
                    ImageUrl = p.ImageUrl,
                    Category = p.Category,
                    InStock = p.InStock,
                    StockQuantity = p.StockQuantity
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var categories = await _context.Products
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost]
        [Authorize] // Add role-based authorization as needed
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto request)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                WholesalePrice = request.WholesalePrice,
                RetailPrice = request.RetailPrice,
                ImageUrl = request.ImageUrl,
                Category = request.Category,
                InStock = request.InStock,
                StockQuantity = request.StockQuantity
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                WholesalePrice = product.WholesalePrice,
                RetailPrice = product.RetailPrice,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                InStock = product.InStock,
                StockQuantity = product.StockQuantity
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
        }
    }
}