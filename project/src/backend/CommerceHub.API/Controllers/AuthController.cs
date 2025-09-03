using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommerceHub.API.Data;
using CommerceHub.API.Models;
using CommerceHub.API.DTOs;
using CommerceHub.API.Services;

namespace CommerceHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CommerceHubContext _context;
        private readonly IAuthService _authService;

        public AuthController(CommerceHubContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == request.Email);

            if (customer == null || !_authService.VerifyPassword(request.Password, customer.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var token = _authService.GenerateJwtToken(customer);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Customer = new CustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    ClientType = customer.ClientType,
                    CreatedAt = customer.CreatedAt
                }
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
        {
            // Check if customer already exists
            if (await _context.Customers.AnyAsync(c => c.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists" });
            }

            // Validate client type
            if (request.ClientType != "wholesaler" && request.ClientType != "retailer")
            {
                return BadRequest(new { message = "Invalid client type" });
            }

            var customer = new Customer
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = _authService.HashPassword(request.Password),
                ClientType = request.ClientType
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(customer);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Customer = new CustomerDto
                {
                    Id = customer.Id,
                    Name = customer.Name,
                    Email = customer.Email,
                    ClientType = customer.ClientType,
                    CreatedAt = customer.CreatedAt
                }
            });
        }
    }
}