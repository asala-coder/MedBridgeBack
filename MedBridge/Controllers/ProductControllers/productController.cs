using MedBridge.Dtos.Product;
using MedBridge.Dtos.ProductADD;
using MedBridge.Models.ProductModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;
using System.Linq;

namespace MedBridge.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class productController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "assets", "images");
        private readonly string _baseUrl = "https://10.0.2.2:7273"; // Replace with your actual base URL

        public productController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private readonly List<string> _allowedExtensions = new List<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".svg", ".ico", ".heif"
        };

        private readonly double _maxAllowedImageSize = 10 * 1024 * 1024; 

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] ProductADDDto dto)
        {
            // Validate Category
            if (!await _dbContext.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
                return BadRequest("Invalid Category ID.");

            // Validate SubCategory
            if (!await _dbContext.subCategories.AnyAsync(s => s.SubCategoryId == dto.SubCategoryId && s.CategoryId == dto.CategoryId))
                return BadRequest("Invalid or mismatched SubCategory ID.");

            // Validate User
            if (!await _dbContext.users.AnyAsync(u => u.Id == dto.UserId))
                return BadRequest("Invalid User ID.");

            var imageUrls = new List<string>();
            foreach (var image in dto.Images)
            {
                var ext = Path.GetExtension(image.FileName).ToLower();
                if (!_allowedExtensions.Contains(ext))
                    return BadRequest("Unsupported image format.");

                if (image.Length > _maxAllowedImageSize)
                    return BadRequest("Image size exceeds 10 MB.");

                var fileName = Guid.NewGuid() + ext;
                var savePath = Path.Combine(_imageUploadPath, fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrls.Add($"{_baseUrl}/images/{fileName}");
            }

            var product = new ProductModel
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsNew = dto.IsNew,
                StockQuantity = dto.StockQuantity,
                Discount = dto.Discount,
                SubCategoryId = dto.SubCategoryId,
                CategoryId = dto.CategoryId,
                UserId = dto.UserId,
                ImageUrls = imageUrls
            };

            try
            {
                await _dbContext.Products.AddAsync(product);
                await _dbContext.SaveChangesAsync();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var products = await _dbContext.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] ProductADDDto dto)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Discount = dto.Discount;
            product.IsNew = dto.IsNew;
            product.CategoryId = dto.CategoryId;
            product.SubCategoryId = dto.SubCategoryId;

            // Optional: handle new images if uploaded
            if (dto.Images != null && dto.Images.Any())
            {
                var imageUrls = new List<string>();
                foreach (var image in dto.Images)
                {
                    var ext = Path.GetExtension(image.FileName).ToLower();
                    if (!_allowedExtensions.Contains(ext))
                        return BadRequest("Unsupported image format.");

                    if (image.Length > _maxAllowedImageSize)
                        return BadRequest("Image size exceeds 10 MB.");

                    var fileName = Guid.NewGuid() + ext;
                    var savePath = Path.Combine(_imageUploadPath, fileName);

                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    imageUrls.Add($"{_baseUrl}/images/{fileName}");
                }

                // Replace old images with new ones
                product.ImageUrls = imageUrls;
            }

            await _dbContext.SaveChangesAsync();
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null)
                return NotFound($"Product with ID {id} not found.");

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();

            return Ok("Product deleted successfully.");
        }
   
//[HttpPut("{id}")]


//        public async Task<IActionResult> updateAsync(int id, [FromForm] ProductDto dto)
//        {
//            var product = await _dbContext.Products.FindAsync(id);

//            if (product == null)
//                return NotFound($"ID {id} not found");


//            //if (dto.Image != null)
//            //{


//            //    if (!_allowedExtensions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
//            //        return BadRequest("only png and jpg images are allowed");
//            //    if (dto.Image.Length > _maxAllowedPosterSize)
//            //        return BadRequest("Max Allowed size for poster is 1 MB");

//            //    using var stream = new MemoryStream();

//            //    await dto.Image.CopyToAsync(stream);

//            //    category.Image = stream.ToArray();
//            //}


//            product.Name = dto.Name;

//            product.Description = dto.Description;
//            product.Price = dto.Price;
//            product.Discount = dto.Discount;
//            product.SubCategoryId = dto.SubCategoryId;
//            product.CategoryId = dto.CategoryId;
//            product.IsNew = dto.IsNew;
//            _dbContext.SaveChanges();
//            return Ok(product);

//        }

        //[HttpDelete("{id}")]


        //public async Task<IActionResult> deleteAsync(int id)
        //{

        //    var product = await _dbContext.Products.FindAsync(id);

        //    if (product == null)
        //        return NotFound($"ID {id} not found");


        //    _dbContext.Remove(product);
        //    _dbContext.SaveChanges();

        //    return Ok(product);
        //}
    }
}
