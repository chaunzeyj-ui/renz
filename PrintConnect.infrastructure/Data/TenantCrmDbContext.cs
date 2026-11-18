using Microsoft.EntityFrameworkCore;
using PrintConnect.domain.entities;
using PrintConnect.domain.Entities;

namespace PrintConnect.infrastructure.Data
{
    public class TenantCrmDbContext : DbContext
    {
        public TenantCrmDbContext(DbContextOptions<TenantCrmDbContext> options) : base(options) { }

        // Tables
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Inquiry> Inquiries => Set<Inquiry>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();       // Added
        public DbSet<Inventory> Inventories => Set<Inventory>();   // Added

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Product Configuration
            builder.Entity<Product>(entity =>
            {
                entity.HasKey(x => x.ProductId);
                entity.Property(x => x.ProductId).ValueGeneratedOnAdd();
                entity.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.HasIndex(x => x.ProductCode).IsUnique();
            });

            // Inquiry Configuration
            builder.Entity<Inquiry>(entity =>
            {
                entity.HasKey(x => x.InquiryId);
                entity.Property(x => x.InquiryId).ValueGeneratedOnAdd();
                entity.Property(x => x.CustomerName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Subject).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            });

            // Customer Configuration
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(x => x.CustomerId);
                entity.Property(x => x.CustomerId).ValueGeneratedOnAdd();
                entity.Property(x => x.CustomerName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.EmailAddress).HasMaxLength(150);
                entity.Property(x => x.PhoneNumber).HasMaxLength(50);
            });

            // Order Configuration
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.OrderId);
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
                entity.HasOne(x => x.Customer)
                      .WithMany()
                      .HasForeignKey(x => x.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem Configuration
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(x => x.OrderItemId);
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.HasOne(x => x.Order)
                      .WithMany(x => x.OrderItems)
                      .HasForeignKey(x => x.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Supplier Configuration
            builder.Entity<Supplier>(entity =>
            {
                entity.HasKey(x => x.SupplierId);
                entity.Property(x => x.SupplierId).ValueGeneratedOnAdd();
                entity.Property(x => x.SupplierCode).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.SupplierCode).IsUnique();
            });

            // Inventory Configuration
            builder.Entity<Inventory>(entity =>
            {
                entity.HasKey(x => x.InventoryId);
                entity.Property(x => x.InventoryId).ValueGeneratedOnAdd();
                entity.Property(x => x.QuantityOnHand).HasPrecision(18, 2);
                entity.Property(x => x.ReorderLevel).HasPrecision(18, 2);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}