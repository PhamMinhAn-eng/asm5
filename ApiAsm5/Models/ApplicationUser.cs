
// File: Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace ASM5.Models
{
    public class ApplicationUser : IdentityUser<string>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
        }
        // Thêm các thuộc tính bổ sung nếu cần, ví dụ:
        public string FullName { get; set; }
        public string Address { get; set; }
        public string TinhTrangHoatDong { get; set; }


        // Navigation properties cho nghiệp vụ
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
