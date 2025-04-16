using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using ASM5.Models; // Nếu bạn có view models, bạn có thể dùng model ProductViewModel hoặc sử dụng chung model Product từ ASM5.Models
using System.Net.Http.Headers;

namespace ASM5.MVC.Controllers
{
    public class StaffController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StaffController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Index()
        {
            return View();
        }
        // Hiển thị trang quản lý sản phẩm
        public async Task<IActionResult> ProductManagement()
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync("api/products/AllPro");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách sản phẩm.";
                return View(new List<ASM5.Models.Product>());
            }
            var json = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ASM5.Models.Product>>(json);
            return View(products);
        }

        // Tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string searchTerm)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync($"api/products/search?searchTerm={searchTerm}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return View("ProductManagement", new List<ASM5.Models.Product>());
            }
            var json = await response.Content.ReadAsStringAsync();
            var products = JsonConvert.DeserializeObject<List<ASM5.Models.Product>>(json);
            return View("ProductManagement", products);
        }

        // Thêm sản phẩm mới
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductCreateModel model)
        {
            if (model.ProductImage == null)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ảnh.";
                return RedirectToAction("ProductManagement");
            }

            var client = _httpClientFactory.CreateClient("APIClient");
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.ProductName), "ProductName");
            content.Add(new StringContent(model.Price.ToString()), "Price");
            content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
            content.Add(new StringContent(model.Color), "Color");
            content.Add(new StringContent(model.Size), "Size");
            content.Add(new StringContent(model.Description), "Description");
            content.Add(new StringContent(model.CategoryID.ToString()), "CategoryID");

            if (model.ProductImage != null)
            {
                using var ms = new MemoryStream();
                await model.ProductImage.CopyToAsync(ms);
                ms.Position = 0;
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProductImage.ContentType);
                content.Add(fileContent, "ProductImage", model.ProductImage.FileName);
            }

            var response = await client.PostAsync("api/products", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
            else
                TempData["ErrorMessage"] = "Thêm sản phẩm thất bại!";
            return RedirectToAction("ProductManagement");
        }

        // Ngừng bán sản phẩm
        [HttpPost]
        public async Task<IActionResult> StopSelling(int ProductID)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("ProductID", ProductID.ToString())
            });
            var response = await client.PostAsync("api/products/stop", formContent);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Sản phẩm đã được ngừng bán!";
            else
                TempData["ErrorMessage"] = "Thao tác thất bại!";
            return RedirectToAction("ProductManagement");
        }

        // Kích hoạt lại sản phẩm
        [HttpPost]
        public async Task<IActionResult> Activate1(int ProductID)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("ProductID", ProductID.ToString())
            });
            var response = await client.PostAsync("api/products/activate", formContent);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Sản phẩm đã được kích hoạt lại!";
            else
                TempData["ErrorMessage"] = "Thao tác thất bại!";
            return RedirectToAction("ProductManagement");
        }

        // Cập nhật sản phẩm
        [HttpPost]
        public async Task<IActionResult> UpdateProduct(ProductUpdateModel model)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(model.ProductID.ToString()), "ProductID");
            content.Add(new StringContent(model.ProductName), "ProductName");
            content.Add(new StringContent(model.Price.ToString()), "Price");
            content.Add(new StringContent(model.Quantity.ToString()), "Quantity");
            content.Add(new StringContent(model.Color), "Color");
            content.Add(new StringContent(model.Size), "Size");
            content.Add(new StringContent(model.Description), "Description");

            if (model.ProductImage != null)
            {
                using var ms = new MemoryStream();
                await model.ProductImage.CopyToAsync(ms);
                ms.Position = 0;
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(model.ProductImage.ContentType);
                content.Add(fileContent, "ProductImage", model.ProductImage.FileName);
            }

            var response = await client.PutAsync("api/products", content);
            if (response.IsSuccessStatusCode)
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            else
                TempData["ErrorMessage"] = "Cập nhật sản phẩm thất bại!";
            return RedirectToAction("ProductManagement");
        }
        public async Task<IActionResult> EmployeeManagement()
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var response = await client.GetAsync("api/employee");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được danh sách nhân viên.";
                return View(new List<User>());
            }
            var json = await response.Content.ReadAsStringAsync();
            var employees = JsonConvert.DeserializeObject<List<User>>(json);
            return View(employees);
        }


        // Thêm nhân viên mới
        [HttpPost]
        public async Task<IActionResult> AddEmployee(User user, string password)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            using var formData = new MultipartFormDataContent();
            formData.Add(new StringContent(user.FullName), "FullName");
            formData.Add(new StringContent(user.Email), "Email");
            formData.Add(new StringContent(password), "Password");
            formData.Add(new StringContent(user.Address), "Address");
            formData.Add(new StringContent(user.PhoneNumber), "PhoneNumber");

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
        public async Task<IActionResult> UpdateRole(string UserID)
        {
            var client = _httpClientFactory.CreateClient("APIClient");
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("UserID", UserID)
            });
            var response = await client.PostAsync("api/employee/updaterole", formData);
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Update role thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Update role thất bại.";
            }
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
