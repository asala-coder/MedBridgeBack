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

        public productController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private double _maxAllowedPosterSize = 1048576;



        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] ProductDto dto)
        {
            //if (dto.Image != null)
            //{
            //    var allowedExtensions = new List<string> { ".jpg", ".png" };
            //    var extension = Path.GetExtension(dto.Image.FileName).ToLower();

            //    if (!allowedExtensions.Contains(extension))
            //        return BadRequest("Only PNG and JPG images are allowed.");

            //    if (dto.Image.Length > 1048576)
            //        return BadRequest("Max allowed size for image is 1 MB.");

            //    using var stream = new MemoryStream();
            //    await dto.Image.CopyToAsync(stream);
            //}


            bool validateCategoryID = await _dbContext.Categories.AnyAsync(g => g.CategoryId == dto.CategoryId);

            if (!validateCategoryID)
                return BadRequest("Not validate Category ID");

            bool validateSubCategoryID = await _dbContext.subCategories
            .AnyAsync(s => s.SubCategoryId == dto.SubCategoryId && s.CategoryId == dto.CategoryId);

            var subCat = await _dbContext.subCategories
    .Where(s => s.SubCategoryId == dto.SubCategoryId)
    .FirstOrDefaultAsync();

            if (subCat != null)
            {
                Console.WriteLine($"Found SubCategory with CategoryId = {subCat.CategoryId}");
            }
            else
            {
                Console.WriteLine("SubCategory not found at all.");
            }



            if (!validateSubCategoryID)
                return BadRequest("Not validate SubCategory ID");
            ProductModel product = new ProductModel
            {
                ProductId = dto.ProductId,
                Name = dto.Name,
                Description = dto.Description,
                Discount = dto.Discount,
                IsNew = dto.IsNew,
                Price = dto.Price,
                SubCategoryId = dto.SubCategoryId,
                CategoryId = dto.CategoryId,

                UserId = dto.UserId
            };

            await _dbContext.AddAsync(product);
            await _dbContext.SaveChangesAsync();
            return Ok(product);
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
