using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using ASM5.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Asm5.Models;
using System.Net.Http.Headers;
namespace ASMC4.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid) {
                TempData["SuccessMessage"] = "Đăng kýăng nhập!";
                return View(model); }

             

            var client = _httpClientFactory.CreateClient("APIClient");
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5025/api/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Đăng ký thất bại. Vui lòng kiểm tra lại dữ liệu!";
                return View(model);
            }

            TempData["SuccessMessage"] = "Đăng ký thành công, mời đăng nhập!";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("APIClient");
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5025/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Sai email hoặc mật khẩu!";
                return View(model);
            }

            var result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            var token = (string)result.token;

            // Lưu token vào session
            HttpContext.Session.SetString("JWT", token);
            token = HttpContext.Session.GetString("JWT");
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var claims = jwtToken.Claims.ToList();

                // Lấy thông tin từ các claim
                var roleName = claims.FirstOrDefault(c => c.Type == "RoleName")?.Value ?? "";

                var fullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "";
                var email = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value ?? "";
                var userId = claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value ?? "";
                // Nếu bạn thêm RoleID trong token (ví dụ: new Claim("RoleID", user.RoleID.ToString()))
                var roleId = claims.FirstOrDefault(c => c.Type == "RoleID")?.Value ?? "0";

                // Lưu vào Session
                HttpContext.Session.SetString("Role", roleName);
                HttpContext.Session.SetString("Username", fullName);
                HttpContext.Session.SetString("UserID", userId);
                HttpContext.Session.SetString("Email", email);
                HttpContext.Session.SetString("RoleID", roleId);
            }
            var roleName1 = HttpContext.Session.GetString("Role");

            if (roleName1 == "Customer")
            {
                return RedirectToAction("Index", "Products");
            }
            else if (roleName1 == "Employee")
            {
                return RedirectToAction("Index", "Staff");
            }
            else if (roleName1 == "Manager")
            {
                return RedirectToAction("Index", "Staff");
            }
            return RedirectToAction("Index", "Products");
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Products");
        }

        // GET: Hiển thị thông tin cá nhân
        public async Task<IActionResult> ManageProfile()
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("APIClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/auth/GetProfile");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không lấy được thông tin người dùng.";
                return RedirectToAction("Index", "Products");
            }
            var json = await response.Content.ReadAsStringAsync();
            var userProfile = JsonConvert.DeserializeObject<UserProfileViewModel>(json);
            return View(userProfile);
        }

        // POST: Cập nhật thông tin cá nhân
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileModel model)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient("APIClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/auth/UpdateProfile", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Cập nhật thất bại.";
            }
            return RedirectToAction("ManageProfile");
        }

        // POST: Đổi mật khẩu
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            var token = HttpContext.Session.GetString("JWT");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return RedirectToAction("ManageProfile");
            }

            var client = _httpClientFactory.CreateClient("APIClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/auth/changepassword", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Thay đổi mật khẩu thất bại.";
            }
            return RedirectToAction("ManageProfile");
        }
    }
}
