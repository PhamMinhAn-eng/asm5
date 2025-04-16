using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM5.Models
{
    public partial class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Image { get; set; }
        public string? Description { get; set; }

        [NotMapped]
        public string CategoryName { get; set; }

        public int? CategoryID { get; set; }
        public string? TinhTrang { get; set; }

        public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
        public virtual Category? Category { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
