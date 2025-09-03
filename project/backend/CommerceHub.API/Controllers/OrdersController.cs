using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CommerceHub.API.Data;
using CommerceHub.API.Models;
using CommerceHub.API.DTOs;

namespace CommerceHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly CommerceHubContext _context;

        public OrdersController(CommerceHubContext context)
        {
            _context = context;
        }

        private int GetCurrentCustomerId()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(customerIdClaim ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var customerId = GetCurrentCustomerId();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ClientType = o.ClientType,
                    OrderDate = o.OrderDate,
                    ShippedDate = o.ShippedDate,
                    DeliveredDate = o.DeliveredDate,
                    ShippingAddress = o.ShippingAddress,
                    Notes = o.Notes,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Product = new ProductDto
                        {
                            Id = oi.Product.Id,
                            Name = oi.Product.Name,
                            Description = oi.Product.Description,
                            WholesalePrice = oi.Product.WholesalePrice,
                            RetailPrice = oi.Product.RetailPrice,
                            ImageUrl = oi.Product.ImageUrl,
                            Category = oi.Product.Category,
                            InStock = oi.Product.InStock,
                            StockQuantity = oi.Product.StockQuantity
                        }
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var customerId = GetCurrentCustomerId();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Id == id && o.CustomerId == customerId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ClientType = o.ClientType,
                    OrderDate = o.OrderDate,
                    ShippedDate = o.ShippedDate,
                    DeliveredDate = o.DeliveredDate,
                    ShippingAddress = o.ShippingAddress,
                    Notes = o.Notes,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Product = new ProductDto
                        {
                            Id = oi.Product.Id,
                            Name = oi.Product.Name,
                            Description = oi.Product.Description,
                            WholesalePrice = oi.Product.WholesalePrice,
                            RetailPrice = oi.Product.RetailPrice,
                            ImageUrl = oi.Product.ImageUrl,
                            Category = oi.Product.Category,
                            InStock = oi.Product.InStock,
                            StockQuantity = oi.Product.StockQuantity
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto request)
        {
            var customerId = GetCurrentCustomerId();

            // Get customer and cart items
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return BadRequest(new { message = "Customer not found" });
            }

            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return BadRequest(new { message = "Cart is empty" });
            }

            // Calculate total amount based on client type
            var totalAmount = cartItems.Sum(item =>
            {
                var price = customer.ClientType == "wholesaler" ? item.Product.WholesalePrice : item.Product.RetailPrice;
                return price * item.Quantity;
            });

            // Create order
            var order = new Order
            {
                CustomerId = customerId,
                TotalAmount = totalAmount,
                Status = "pending",
                ClientType = customer.ClientType,
                ShippingAddress = request.ShippingAddress,
                Notes = request.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            var orderItems = cartItems.Select(cartItem => new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = customer.ClientType == "wholesaler" ? cartItem.Product.WholesalePrice : cartItem.Product.RetailPrice,
                TotalPrice = (customer.ClientType == "wholesaler" ? cartItem.Product.WholesalePrice : cartItem.Product.RetailPrice) * cartItem.Quantity
            }).ToList();

            _context.OrderItems.AddRange(orderItems);

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            // Return order with items
            var orderDto = new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ClientType = order.ClientType,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                Notes = order.Notes,
                OrderItems = orderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Product = cartItems.First(ci => ci.ProductId == oi.ProductId).Product != null ? new ProductDto
                    {
                        Id = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.Id,
                        Name = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.Name,
                        Description = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.Description,
                        WholesalePrice = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.WholesalePrice,
                        RetailPrice = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.RetailPrice,
                        ImageUrl = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.ImageUrl,
                        Category = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.Category,
                        InStock = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.InStock,
                        StockQuantity = cartItems.First(ci => ci.ProductId == oi.ProductId).Product.StockQuantity
                    } : new ProductDto()
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
        }
    }
}