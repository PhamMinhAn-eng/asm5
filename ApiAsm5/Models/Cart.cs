using System;
using System.Collections.Generic;

namespace ASM5.Models
{
    public partial class Cart
    {
        public int CartId { get; set; }
        public string? CustomerId { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
        public virtual ApplicationUser? Customer { get; set; }
    }
}
