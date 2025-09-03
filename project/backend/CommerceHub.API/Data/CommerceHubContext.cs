using Microsoft.EntityFrameworkCore;
using CommerceHub.API.Models;

namespace CommerceHub.API.Data
{
    public class CommerceHubContext : DbContext
    {
        public CommerceHubContext(DbContextOptions<CommerceHubContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.ClientType).HasConversion<string>();
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.WholesalePrice).HasPrecision(10, 2);
                entity.Property(e => e.RetailPrice).HasPrecision(10, 2);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
                entity.HasOne(e => e.Customer)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.TotalPrice).HasPrecision(12, 2);
                
                entity.HasOne(e => e.Order)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasOne(e => e.Customer)
                      .WithMany(e => e.CartItems)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                      .WithMany(e => e.CartItems)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Ensure unique cart items per customer-product combination
                entity.HasIndex(e => new { e.CustomerId, e.ProductId }).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Premium Coffee Beans",
                    Description = "High-quality arabica coffee beans sourced from sustainable farms",
                    WholesalePrice = 12.50m,
                    RetailPrice = 24.99m,
                    ImageUrl = "https://images.pexels.com/photos/894695/pexels-photo-894695.jpeg",
                    Category = "Beverages",
                    InStock = true,
                    StockQuantity = 100
                },
                new Product
                {
                    Id = 2,
                    Name = "Organic Green Tea",
                    Description = "Premium organic green tea leaves with antioxidant properties",
                    WholesalePrice = 8.75m,
                    RetailPrice = 16.99m,
                    ImageUrl = "https://images.pexels.com/photos/1417945/pexels-photo-1417945.jpeg",
                    Category = "Beverages",
                    InStock = true,
                    StockQuantity = 75
                },
                new Product
                {
                    Id = 3,
                    Name = "Artisan Chocolate Bar",
                    Description = "Handcrafted dark chocolate with 70% cocoa content",
                    WholesalePrice = 3.25m,
                    RetailPrice = 7.99m,
                    ImageUrl = "https://images.pexels.com/photos/918327/pexels-photo-918327.jpeg",
                    Category = "Confectionery",
                    InStock = true,
                    StockQuantity = 200
                },
                new Product
                {
                    Id = 4,
                    Name = "Natural Honey",
                    Description = "Pure, unprocessed honey from local beekeepers",
                    WholesalePrice = 6.50m,
                    RetailPrice = 12.99m,
                    ImageUrl = "https://images.pexels.com/photos/87999/pexels-photo-87999.jpeg",
                    Category = "Natural Products",
                    InStock = true,
                    StockQuantity = 50
                },
                new Product
                {
                    Id = 5,
                    Name = "Gourmet Olive Oil",
                    Description = "Extra virgin olive oil from Mediterranean groves",
                    WholesalePrice = 15.00m,
                    RetailPrice = 28.99m,
                    ImageUrl = "https://images.pexels.com/photos/33783/olive-oil-salad-dressing-cooking-olive.jpg",
                    Category = "Condiments",
                    InStock = true,
                    StockQuantity = 30
                }
            );
        }
    }
}