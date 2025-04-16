using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ASM5.Models; // ApplicationUser
using System.Threading.Tasks;
using System.Linq;

namespace ASM5.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EmployeeController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Lấy danh sách nhân viên
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var users = _userManager.Users.ToList();
            var employeeDtos = new List<EmployeeDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                employeeDtos.Add(new EmployeeDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TinhTrangHoatDong = user.TinhTrangHoatDong,
                    Roles = roles.ToList()
                });
            }

            return Ok(employeeDtos);
        }

        // Thêm nhân viên mới (mặc định role là "Employee")
        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee([FromForm] EmployeeDto model, [FromForm] string password)
        {
            // Kiểm tra email tồn tại
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                return BadRequest(new { message = "Email đã tồn tại." });
            }
            var user = new ApplicationUser
            {
                TinhTrangHoatDong = "on",
                Id = Guid.NewGuid().ToString(),  // Gán Id mới
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber
            
            };
            user.TinhTrangHoatDong = "On";
            // Bạn có thể không cần set RoleName vì Identity quản lý role thông qua UserManager
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            // Gán role "Employee" cho user mới
            if (!await _roleManager.RoleExistsAsync("Employee"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Employee"));
            }
            await _userManager.AddToRoleAsync(user, "Employee");

            return Ok(new { message = "Thêm nhân viên thành công." });
        }

        // Cập nhật vai trò thành Quản lý
        [HttpPost("updaterole")]
        public async Task<IActionResult> UpdateRole([FromForm] string UserID)
        {
            var user = await _userManager.FindByIdAsync(UserID);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            // Nếu user đang có role khác, bạn có thể xoá role cũ (nếu cần)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }
            // Gán role "Manager"
            if (!await _roleManager.RoleExistsAsync("Manager"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Manager"));
            }
            await _userManager.AddToRoleAsync(user, "Manager");
            return Ok(new { message = "Cập nhật vai trò thành Quản lý thành công." });
        }

        // Ngừng hoạt động của nhân viên
        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate([FromForm] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            user.TinhTrangHoatDong = "Off";
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Ngừng hoạt động thành công." });
        }

        // Kích hoạt lại nhân viên
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromForm] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng." });

            user.TinhTrangHoatDong = "On";
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Kích hoạt thành công." });
        }
    }
}
