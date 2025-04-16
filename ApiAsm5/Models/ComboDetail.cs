namespace ASM5.Models
{
    public class ComboDetail
    {
        public int ComboDetailId { get; set; }
        public int ComboId { get; set; }
        public virtual Combo Combo { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
