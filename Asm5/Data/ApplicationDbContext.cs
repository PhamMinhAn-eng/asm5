using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ASM5.Models; // Đảm bảo namespace đúng với các model của bạn

namespace ASM5.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Các DbSet cho nghiệp vụ
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartDetail> CartDetails { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboDetail> ComboDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình mối quan hệ giữa ApplicationUser và Cart (1-N)
            builder.Entity<Cart>()
                .HasOne(c => c.Customer)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.CustomerId);

            // Cấu hình mối quan hệ giữa ApplicationUser và Order (1-N)
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId);

            // Cấu hình cho ComboDetail
            builder.Entity<ComboDetail>()
                .HasKey(cd => cd.ComboDetailId);

            builder.Entity<ComboDetail>()
                .HasOne(cd => cd.Combo)
                .WithMany(c => c.ComboDetails)
                .HasForeignKey(cd => cd.ComboId);

            builder.Entity<ComboDetail>()
                .HasOne(cd => cd.Product)
                .WithMany() // Nếu Product không có navigation property về ComboDetail
                .HasForeignKey(cd => cd.ProductId);

            // Cấu hình cho CartDetail
            builder.Entity<CartDetail>()
                .HasOne(cd => cd.Cart)
                .WithMany(c => c.CartDetails)
                .HasForeignKey(cd => cd.CartId);

            builder.Entity<CartDetail>()
                .HasOne(cd => cd.Product)
                .WithMany(p => p.CartDetails)
                .HasForeignKey(cd => cd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartDetail>()
                .HasOne(cd => cd.Combo)
                .WithMany() // Nếu Combo không có navigation property về CartDetail
                .HasForeignKey(cd => cd.ComboId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình độ chính xác cho các trường decimal

            // CartDetail.UnitPrice: giá của sản phẩm hoặc combo tại thời điểm thêm vào giỏ
            builder.Entity<CartDetail>()
                .Property(cd => cd.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Combo.Price: giá của combo
            builder.Entity<Combo>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            // Order.TotalAmount: tổng tiền của đơn hàng
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // OrderDetail.UnitPrice: giá của sản phẩm trong đơn hàng tại thời điểm đặt
            builder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Product.Price: giá của sản phẩm
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}
