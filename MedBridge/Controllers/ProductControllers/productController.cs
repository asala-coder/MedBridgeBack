using MedBridge.Dtos.Product;
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

        public productController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private List<string> _allowedExtensions = new List<string>
{
    ".jpg",
    ".jpeg",
    ".png",
    ".gif",
    ".bmp",
    ".webp",
    ".tiff",
    ".tif",
    ".svg",
    ".ico",
    ".heif"
};
        private double _maxAllowedPosterSize = 10 * 1024 * 1024; // Max size: 10 MB (10,048,576 bytes)

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] ProductDto dto)
        {
            // Validate CategoryId
            var categoryExists = await _dbContext.Categories
                .AnyAsync(c => c.CategoryId == dto.CategoryId);
            if (!categoryExists) return BadRequest("Category ID does not exist.");

            // Validate SubCategoryId
            var subCategoryExists = await _dbContext.subCategories
                .AnyAsync(s => s.SubCategoryId == dto.SubCategoryId && s.CategoryId == dto.CategoryId);
            if (!subCategoryExists) return BadRequest("SubCategory ID does not exist or does not match Category ID.");

            // Validate UserId
            var userExists = await _dbContext.users
                .AnyAsync(u => u.Id == dto.UserId);
            if (!userExists) return BadRequest("The specified UserId does not exist.");

            // Save images
            var imageUrls = new List<string>();
            foreach (var image in dto.Images)
            {
                var imageExtension = Path.GetExtension(image.FileName).ToLower();
                Console.WriteLine($"Image Extension: {imageExtension}");
                if (!_allowedExtensions.Contains(imageExtension))
                    return BadRequest("Only image files with the following extensions are allowed: " + string.Join(", ", _allowedExtensions));

                if (image.Length > _maxAllowedPosterSize)
                    return BadRequest("Image size should not exceed 1 MB.");

                var imageFileName = Guid.NewGuid().ToString() + imageExtension;
                var imagePath = Path.Combine(_imageUploadPath, imageFileName);

                // Save image to server
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imageUrls.Add($"/images/{imageFileName}");
            }

            // Create Product Model and Save to Database
            var product = new ProductModel
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsNew = dto.IsNew,
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

        public async Task<IActionResult> GetALLAsync()
        {

            var Products = await _dbContext.Products.ToListAsync();

            return Ok(Products);

        }


        [HttpGet("{id}")]

        public async Task<IActionResult> GetByIdLAsync(int id)
        {

            var Product = await _dbContext.Products.FindAsync(id);

            if (Product == null)
                return NotFound();

            return Ok(Product);

        }


        [HttpPut("{id}")]


        public async Task<IActionResult> updateAsync(int id, [FromForm] ProductDto dto)
        {
            var product = await _dbContext.Products.FindAsync(id);

            if (product == null)
                return NotFound($"ID {id} not found");


            //if (dto.Image != null)
            //{


            //    if (!_allowedExtensions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
            //        return BadRequest("only png and jpg images are allowed");
            //    if (dto.Image.Length > _maxAllowedPosterSize)
            //        return BadRequest("Max Allowed size for poster is 1 MB");

            //    using var stream = new MemoryStream();

            //    await dto.Image.CopyToAsync(stream);

            //    category.Image = stream.ToArray();
            //}


            product.Name = dto.Name;

            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Discount = dto.Discount;
            product.SubCategoryId = dto.SubCategoryId;
            product.CategoryId = dto.CategoryId;
            product.IsNew = dto.IsNew;
            _dbContext.SaveChanges();
            return Ok(product);

        }

        [HttpDelete("{id}")]


        public async Task<IActionResult> deleteAsync(int id)
        {

            var product = await _dbContext.Products.FindAsync(id);

            if (product == null)
                return NotFound($"ID {id} not found");


            _dbContext.Remove(product);
            _dbContext.SaveChanges();

            return Ok(product);
        }
    }
}
