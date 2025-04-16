using ASM5.Data;
using ASM5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASMC4.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;
        public ProductsController(ProductRepository productRepository, IWebHostEnvironment env)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _env = env;
        }
        // Lấy danh sách sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productRepository.GetAllProductsAsync();
            return Ok(products);
        }
        [HttpGet("AllPro")]

        public async Task<IActionResult> GetAllProductsAll()
        {
            var products = await _productRepository.AllPro();
            return Ok(products);
        }

        // Tìm kiếm sản phẩm theo tên
        [HttpGet("SearchProducts")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm);
            return Ok(products);
        }

        // Thêm sản phẩm mới (dữ liệu gửi dưới dạng multipart/form-data)
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] ProductCreateModel model)
        {
            if (model.ProductImage == null || model.ProductImage.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn ảnh." });

            // Lấy đường dẫn webRoot
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesFolder = Path.Combine(webRoot, "images");

            // Tạo thư mục images nếu chưa tồn tại
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }
            var fileName = Path.GetFileName(model.ProductImage.FileName);
            var filePath = Path.Combine(webRoot, "images", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProductImage.CopyToAsync(stream);
            }


            var newProduct = new Product
            {
                ProductName = model.ProductName,
                Price = model.Price,
                Quantity = model.Quantity,
                Color = model.Color,
                Size = model.Size,
                Description = model.Description,
                Image = "/images/" + fileName,
                TinhTrang = "On",
                CategoryID = model.CategoryID
            };

            await _productRepository.AddProductAsync(newProduct);
            return Ok(new { message = "Thêm sản phẩm thành công!" });
        }

        // Cập nhật sản phẩm
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductUpdateModel model)
        {
            var product = await _productRepository.GetProductByIdAsync(model.ProductID);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại!" });

            product.ProductName = model.ProductName;
            product.Price = model.Price;
            product.Quantity = model.Quantity;
            product.Color = model.Color;
            product.Size = model.Size;
            product.Description = model.Description;

            if (model.ProductImage != null && model.ProductImage.Length > 0)
            {
                var fileName = Path.GetFileName(model.ProductImage.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProductImage.CopyToAsync(stream);
                }
                product.Image = "/images/" + fileName;
            }

            await _productRepository.UpdateProductAsync(product);
            return Ok(new { message = "Cập nhật sản phẩm thành công!" });
        }

        // Thay đổi trạng thái sản phẩm (Ngừng bán)
        [HttpPost("stop")]
        public async Task<IActionResult> StopSelling([FromForm] int ProductID)
        {
            await _productRepository.UpdateProductStatusAsync(ProductID, "Off");
            return Ok(new { message = "Sản phẩm đã được ngừng bán!" });
        }

        // Thay đổi trạng thái sản phẩm (Kích hoạt lại)
        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromForm] int ProductID)
        {
            await _productRepository.UpdateProductStatusAsync(ProductID, "On");
            return Ok(new { message = "Sản phẩm đã được kích hoạt lại!" });
        }
 

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");
            return Ok(product);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchProducts(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var products = await _productRepository.SearchProductsAsync(searchTerm, categoryId, minPrice, maxPrice);
         
            return Ok(products);
        }
    }
}
