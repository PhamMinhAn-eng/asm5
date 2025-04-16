using ASM5.Data;
using ASM5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM5.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost("add")]
        public IActionResult AddToCart([FromForm] int productId, [FromForm] int quantity, [FromForm] string customerId)
        {
            
            // Ở API, bạn có thể xác thực bằng token (ở đây demo đơn giản)
            // Nếu chưa có giỏ hàng, tạo mới
            var cart = _context.Carts.FirstOrDefault(c => c.CustomerId == customerId);
            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    CreatedDate = DateTime.Now
                };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var cartDetail = _context.CartDetails.FirstOrDefault(cd => cd.CartId == cart.CartId && cd.ProductId == productId);
            if (cartDetail != null)
            {
                cartDetail.Quantity += quantity;
            }
            else
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product == null)
                {
                    return BadRequest("Sản phẩm không tồn tại.");
                }
                cartDetail = new CartDetail
                {
                    CartId = cart.CartId,
                    Quantity = quantity,
                    ProductId = productId,
                    UnitPrice = product.Price
                };
                _context.CartDetails.Add(cartDetail);
            }
            _context.SaveChanges();
            return Ok(new { message = "Đã thêm vào giỏ hàng." });
        }

        // Lấy giỏ hàng của người dùng (dựa vào customerId)
        [HttpGet("get")]
        public IActionResult GetCart([FromQuery] string customerId)
        {
            var cart = _context.Carts
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                .FirstOrDefault(c => c.CustomerId == customerId);
            if (cart == null || !cart.CartDetails.Any())
            {
                return Ok(new List<CartDetail>());
            }
            return Ok(cart.CartDetails.ToList());
        }

        // Thanh toán giỏ hàng
        [HttpPost("checkout")]
        public IActionResult Checkout([FromForm] string customerId)
        {
            try
            {
                var cart = _context.Carts.FirstOrDefault(c => c.CustomerId == customerId);
                if (cart == null)
                    return BadRequest("Giỏ hàng không tồn tại.");

                var cartItems = _context.CartDetails
                    .Include(cd => cd.Product)
                    .Where(cd => cd.CartId == cart.CartId)
                    .ToList();
                if (!cartItems.Any())
                    return BadRequest("Giỏ hàng trống.");

                var newOrder = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price),
                    Status = "Đang xử lý"
                };
                _context.Orders.Add(newOrder);
                _context.SaveChanges();

                foreach (var item in cartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = newOrder.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price,
                    };
                    _context.OrderDetails.Add(orderDetail);
                }
                _context.SaveChanges();

                _context.CartDetails.RemoveRange(cartItems);
                _context.SaveChanges();

                return Ok(new { message = "Đặt hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        // Xóa một sản phẩm khỏi giỏ hàng
        [HttpPost("remove")]
        public IActionResult RemoveFromCart([FromForm] int id)
        {
            var cartDetail = _context.CartDetails.Find(id);
            if (cartDetail != null)
            {
                _context.CartDetails.Remove(cartDetail);
                _context.SaveChanges();
            }
            return Ok(new { message = "Xóa thành công" });
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng (JSON)
        [HttpPost("updatequantity")]
        public IActionResult UpdateQuantity([FromBody] UpdateCartDetailModel model)
        {
            var cartDetail = _context.CartDetails.FirstOrDefault(cd => cd.CartDetailId == model.CartDetailId);
            if (cartDetail != null)
            {
                cartDetail.Quantity = model.Quantity;
                _context.SaveChanges();
                return Ok(new { message = "Cập nhật thành công" });
            }
            return BadRequest(new { message = "Cập nhật thất bại" });
        }
    }

    public class UpdateCartDetailModel
    {
        public int CartDetailId { get; set; }
        public int Quantity { get; set; }
    }
}
