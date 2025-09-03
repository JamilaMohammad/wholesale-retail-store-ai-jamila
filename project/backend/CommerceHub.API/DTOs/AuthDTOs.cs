using System.ComponentModel.DataAnnotations;

namespace CommerceHub.API.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequestDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ClientType { get; set; } = string.Empty; // "wholesaler" or "retailer"
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public CustomerDto Customer { get; set; } = null!;
    }

    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ClientType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}