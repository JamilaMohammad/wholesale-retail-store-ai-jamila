using System.ComponentModel.DataAnnotations;

namespace CommerceHub.API.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal WholesalePrice { get; set; }
        public decimal RetailPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool InStock { get; set; }
        public int StockQuantity { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal WholesalePrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal RetailPrice { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        public bool InStock { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } = 0;
    }
}