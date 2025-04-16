using System.Collections.Generic;

namespace ASM5.Models
{
    public class Combo
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }

        // Danh sách chi tiết sản phẩm trong combo
        public virtual ICollection<ComboDetail> ComboDetails { get; set; } = new List<ComboDetail>();
    }
}
