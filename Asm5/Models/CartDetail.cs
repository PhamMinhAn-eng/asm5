namespace ASM5.Models
{
    public partial class CartDetail
    {
        public int CartDetailId { get; set; }
        public int CartId { get; set; }

        // Nếu là sản phẩm riêng lẻ, ProductId khác null; nếu là combo, ComboId khác null.
        public int? ProductId { get; set; }
        public int? ComboId { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual Cart Cart { get; set; }
        public virtual Product? Product { get; set; }
        public virtual Combo? Combo { get; set; }
    }
}
