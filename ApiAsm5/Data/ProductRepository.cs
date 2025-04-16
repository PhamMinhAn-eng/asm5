using ASM5.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM5.Data
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.TinhTrang == "On")
                .Select(p => new Product
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Color = p.Color,
                    Size = p.Size,
                    Image = p.Image,
                    Description = p.Description,
                    TinhTrang = p.TinhTrang,
                    CategoryName = p.Category.CategoryName
                })
                .ToListAsync();
        }
        public async Task<List<Product>> AllPro()
        {
            return await _context.Products
                .Include(p => p.Category)
              
                .Select(p => new Product
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Color = p.Color,
                    Size = p.Size,
                    Image = p.Image,
                    Description = p.Description,
                    TinhTrang = p.TinhTrang,
                    CategoryName = p.Category.CategoryName
                })
                .ToListAsync();
        }


        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
        }

        public async Task<List<Product>> SearchProductsAsync(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.ProductName.Contains(searchTerm));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.Where(p => p.TinhTrang == "On").ToListAsync();
        }
        public async Task<List<Product>> GetAllProductsAsync2()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _context.Products
                .Where(p => p.ProductName.Contains(searchTerm))
                .ToListAsync();
        }

      

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProductStatusAsync(int productId, string status)
        {
            var product = await GetProductByIdAsync(productId);
            if (product != null)
            {
                product.TinhTrang = status;
                await _context.SaveChangesAsync();
            }
        }
    }
}
