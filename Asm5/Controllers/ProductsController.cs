using ASM5.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Web;

namespace ASMC4.MVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("http://localhost:5025/api/products");
            if (!response.IsSuccessStatusCode)
            {

                return View(new List<Product>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<Product>>(json);
            var groupedProducts = products.GroupBy(p => p.CategoryName);

            return View(groupedProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var response = await _httpClient.GetAsync($"http://localhost:5025/api/products/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var product = JsonConvert.DeserializeObject<Product>(json);
            return View(product);
        }

        public async Task<IActionResult> Search(string searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            // URL cơ bản của API search
            var url = "http://localhost:5025/api/products/search";

            // Tạo danh sách các tham số query
            var parameters = new List<string>();

            if (!string.IsNullOrEmpty(searchTerm))
                parameters.Add("searchTerm=" + Uri.EscapeDataString(searchTerm));
            if (categoryId.HasValue)
                parameters.Add("category=" + categoryId.Value);
            if (minPrice.HasValue)
                parameters.Add("minPrice=" + minPrice.Value);
            if (maxPrice.HasValue)
                parameters.Add("maxPrice=" + maxPrice.Value);

            // Nếu có tham số nào, thêm dấu ? và nối chúng với &
            if (parameters.Any())
            {
                url += "?" + string.Join("&", parameters);
            }

            // Gọi API với URL đã xây dựng
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return View("Index", new List<Product>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<Product>>(json);

            // Nhóm sản phẩm theo CategoryName (nếu cần hiển thị dạng Group)
            var groupedProducts = products.GroupBy(p => p.CategoryName);

            return View("Index", groupedProducts);
        }

    }
}
