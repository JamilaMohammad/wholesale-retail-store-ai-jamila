using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CommerceHub.API.Data;
using CommerceHub.API.DTOs;
using CommerceHub.API.Models;

namespace CommerceHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly CommerceHubContext _context;

        public CartController(CommerceHubContext context)
        {
            _context = context;
        }

        private int GetCurrentCustomerId()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(customerIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<CartSummaryDto>> GetCart()
        {
            var customerId = GetCurrentCustomerId();

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CustomerId == customerId)
                .Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    CreatedAt = ci.CreatedAt,
                    Product = new ProductDto
                    {
                        Id = ci.Product.Id,
                        Name = ci.Product.Name,
                        Description = ci.Product.Description,
                        WholesalePrice = ci.Product.WholesalePrice,
                        RetailPrice = ci.Product.RetailPrice,
                        ImageUrl = ci.Product.ImageUrl,
                        Category = ci.Product.Category,
                        InStock = ci.Product.InStock,
                        StockQuantity = ci.Product.StockQuantity
                    }
                })
                .ToListAsync();

            var customer = await _context.Customers.FindAsync(customerId);
            var clientType = customer?.ClientType ?? "retailer";

            var totalAmount = cartItems.Sum(item =>
            {
                var price = clientType == "wholesaler" ? item.Product.WholesalePrice : item.Product.RetailPrice;
                return price * item.Quantity;
            });

            var totalItems = cartItems.Sum(item => item.Quantity);

            return Ok(new CartSummaryDto
            {
                Items = cartItems,
                TotalAmount = totalAmount,
                TotalItems = totalItems
            });
        }

        [HttpPost("items")]
        public async Task<ActionResult<CartItemDto>> AddToCart(AddToCartDto request)
        {
            var customerId = GetCurrentCustomerId();

            // Check if product exists and is in stock
            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            if (!product.InStock || product.StockQuantity < request.Quantity)
            {
                return BadRequest(new { message = "Insufficient stock" });
            }

            // Check if item already exists in cart
            var existingCartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CustomerId == customerId && ci.ProductId == request.ProductId);

            if (existingCartItem != null)
            {
                // Update quantity
                existingCartItem.Quantity += request.Quantity;
                existingCartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new cart item
                existingCartItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(existingCartItem);
            }

            await _context.SaveChangesAsync();

            // Return the cart item with product details
            var cartItemDto = new CartItemDto
            {
                Id = existingCartItem.Id,
                ProductId = existingCartItem.ProductId,
                Quantity = existingCartItem.Quantity,
                CreatedAt = existingCartItem.CreatedAt,
                Product = new ProductDto
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
                }
            };

            return Ok(cartItemDto);
        }

        [HttpPut("items/{id}")]
        public async Task<ActionResult<CartItemDto>> UpdateCartItem(int id, UpdateCartItemDto request)
        {
            var customerId = GetCurrentCustomerId();

            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.CustomerId == customerId);

            if (cartItem == null)
            {
                return NotFound();
            }

            // Check stock availability
            if (cartItem.Product.StockQuantity < request.Quantity)
            {
                return BadRequest(new { message = "Insufficient stock" });
            }

            cartItem.Quantity = request.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var cartItemDto = new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                CreatedAt = cartItem.CreatedAt,
                Product = new ProductDto
                {
                    Id = cartItem.Product.Id,
                    Name = cartItem.Product.Name,
                    Description = cartItem.Product.Description,
                    WholesalePrice = cartItem.Product.WholesalePrice,
                    RetailPrice = cartItem.Product.RetailPrice,
                    ImageUrl = cartItem.Product.ImageUrl,
                    Category = cartItem.Product.Category,
                    InStock = cartItem.Product.InStock,
                    StockQuantity = cartItem.Product.StockQuantity
                }
            };

            return Ok(cartItemDto);
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var customerId = GetCurrentCustomerId();

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.CustomerId == customerId);

            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var customerId = GetCurrentCustomerId();

            var cartItems = await _context.CartItems
                .Where(ci => ci.CustomerId == customerId)
                .ToListAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}