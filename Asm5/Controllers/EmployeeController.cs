using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ASM5.Models; // Sử dụng model ApplicationUser (User) từ ASM5.Models
using System.Text;
using System.Net.Http.Headers;

namespace ASM5.MVC.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public EmployeeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Hiển thị danh sách nhân viên
        public async Task<IActionResult> EmployeeManagement()
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync("api/employee");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách nhân viên.";
                return View(new List<ApplicationUser>());
            }
            var json = await response.Content.ReadAsStringAsync();
            var users = JsonConvert.DeserializeObject<List<ApplicationUser>>(json);
            return View(users);
        }

        // Thêm nhân viên mới
        [HttpPost]
        public async Task<IActionResult> AddEmployee(ApplicationUser user, string password)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(user.FullName), "FullName");
            formData.Add(new StringContent(user.Email), "Email");
            formData.Add(new StringContent(password), "Password");
            formData.Add(new StringContent(user.Address), "Address");
            formData.Add(new StringContent(user.PhoneNumber), "Phone");

            var response = await client.PostAsync("api/employee/add", formData);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Thêm nhân viên thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Thêm nhân viên thất bại.";
            }
            return RedirectToAction("EmployeeManagement");
        }

        // Cập nhật vai trò thành Quản lý
        [HttpPost]
        public async Task<IActionResult> UpdateRole(string userId)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserID", userId)
            });
            var response = await client.PostAsync("api/employee/updaterole", formData);
            return RedirectToAction("EmployeeManagement");
        }

        // Ngừng hoạt động
        [HttpPost]
        public async Task<IActionResult> Deactivate(string userId)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserID", userId)
            });
            var response = await client.PostAsync("api/employee/deactivate", formData);
            return RedirectToAction("EmployeeManagement");
        }

        // Kích hoạt lại
        [HttpPost]
        public async Task<IActionResult> Activate(string userId)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserID", userId)
            });
            var response = await client.PostAsync("api/employee/activate", formData);
            return RedirectToAction("EmployeeManagement");
        }
    }
}
