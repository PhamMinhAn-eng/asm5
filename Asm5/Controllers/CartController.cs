using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using ASM5.Models; // Sử dụng model CartDetail, Product, ...
using System.Net.Http;

namespace ASM5.MVC.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CartController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Hiển thị giỏ hàng của người dùng
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync($"api/cart/get?customerId={userId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được giỏ hàng.";
                return View(new List<CartDetail>());
            }
            var json = await response.Content.ReadAsStringAsync();
            var cartDetails = JsonConvert.DeserializeObject<List<CartDetail>>(json);
            return View(cartDetails);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity, string customerId)
        {
            var userId = HttpContext.Session.GetString("UserID");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
          
            var client = _httpClientFactory.CreateClient("APIClient");
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(productId.ToString()), "productId");
            content.Add(new StringContent(quantity.ToString()), "quantity");
            content.Add(new StringContent(customerId.ToString()), "customerId");

            var response = await client.PostAsync("api/cart/add", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào giỏ hàng.";
            else
                TempData["ErrorMessage"] = "Thêm vào giỏ hàng thất bại.";
            return RedirectToAction("Index");
        }

        // Thanh toán giỏ hàng
        [HttpPost]
        public async Task<IActionResult> Checkout(string customerId)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("customerId", customerId.ToString())
            });
            var response = await client.PostAsync("api/cart/checkout", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Đặt hàng thành công!";
            else
                TempData["ErrorMessage"] = "Đặt hàng thất bại!";
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            using var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("id", id.ToString())
            });
            var response = await client.PostAsync("api/cart/remove", formContent);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công.";
            else
                TempData["ErrorMessage"] = "Xóa sản phẩm thất bại.";
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng (gọi qua AJAX)
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartDetailModel model)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/cart/updatequantity", content);
            if (response.IsSuccessStatusCode)
                return Ok(new { message = "Cập nhật thành công" });
            return BadRequest(new { message = "Cập nhật thất bại" });
        }
        public class UpdateCartDetailModel
        {
            public int CartDetailId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
