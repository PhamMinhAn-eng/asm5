using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using ASM5.Models; // Sử dụng chung model Order từ ASM5.Models (nếu view dùng model Order)
using System.Net.Http.Headers;

namespace ASM5.MVC.Controllers
{
    public class OrderManagerController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public OrderManagerController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Hiển thị danh sách đơn hàng (gọi API)
        public async Task<IActionResult> Index(string? status, string? search)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            // Xây dựng URL với query string nếu có
            var url = "api/OrderManager/orders";
            if (!string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(search))
            {
                url += $"?status={status}&search={search}";
            }

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách đơn hàng.";
                return View(new List<Order>());
            }
            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(json);
            return View(orders);
        }

        // Hiển thị chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync($"api/OrderManager/orders/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }
            var json = await response.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<Order>(json);
            return View(order);
        }

        // Cập nhật trạng thái đơn hàng (nếu cần, ví dụ khi admin thay đổi trạng thái)
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string newStatus)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", id.ToString()),
                new KeyValuePair<string, string>("newStatus", newStatus)
            });
            var response = await client.PostAsync("api/OrderManager/orders/update", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Cập nhật trạng thái thất bại.";
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> OrderHistory()
        {
            var customerId = HttpContext.Session.GetString("UserID");
            if (customerId == null)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để xem lịch sử đơn hàng.";
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("APIClient");
            // Gọi API endpoint để lấy đơn hàng theo customerId
            var response = await client.GetAsync($"api/OrderManager/bycustomer?customerId={customerId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được lịch sử đơn hàng.";
                return View(new List<Order>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(json);
            return View(orders);
        }

        // Hiển thị chi tiết đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            // Gọi API endpoint trả về chi tiết đơn hàng theo id
            var response = await client.GetAsync($"api/OrderManager/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("OrderHistory");
            }
            var json = await response.Content.ReadAsStringAsync();
            var order = JsonConvert.DeserializeObject<Order>(json);
            return View(order);
        }
    }
}
