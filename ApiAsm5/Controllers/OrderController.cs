using ASM5.Data;
using ASM5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASM5.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderManagerController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Endpoint trả về đơn hàng theo customerId
        [HttpGet("bycustomer")]
        public IActionResult GetOrdersByCustomerId([FromQuery] string customerId)
        {
            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .Where(o => o.CustomerId == customerId)
                .ToList();

            return Ok(orders);
        }

        // Endpoint trả về chi tiết đơn hàng theo id
        [HttpGet("{id}")]
        public IActionResult GetOrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            return Ok(order);
        }

        // Lấy danh sách đơn hàng (có thể lọc theo trạng thái và tìm kiếm)
        [HttpGet("orders")]
        public IActionResult GetOrders([FromQuery] string? status, [FromQuery] string? search)
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    o.OrderId.ToString().Contains(search) ||
                    o.Customer.FullName.Contains(search));
            }

            return Ok(orders.ToList());
        }

        // Lấy chi tiết đơn hàng theo id
        [HttpGet("orders/{id}")]
        public IActionResult GetOrderDetails1(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            return Ok(order);
        }

        // Cập nhật trạng thái đơn hàng (nếu cần)
        [HttpPost("orders/update")]
        public IActionResult UpdateOrderStatus([FromForm] int id, [FromForm] string newStatus)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng." });
            }

            order.Status = newStatus;
            _context.SaveChanges();

            return Ok(new { message = "Cập nhật trạng thái thành công." });
        }
    }
}
