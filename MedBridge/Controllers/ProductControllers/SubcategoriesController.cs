using MedBridge.Dtos.Product;
using MedBridge.Models.ProductModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;

namespace MedBridge.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public SubcategoriesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private double _maxAllowedPosterSize = 1048576;

    

        [HttpPost]
        public async Task<IActionResult> CreateAsync(subCategoriesDto dto)
        {

            if (dto.Image == null)
                return BadRequest("image is required");
            if (!_allowedExtensions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                return BadRequest("only png and jpg images are allowed");
            if (dto.Image.Length > _maxAllowedPosterSize)
                return BadRequest("Max Allowed size for image is 1 MB");

            using var stream = new MemoryStream();

            await dto.Image.CopyToAsync(stream);

            bool validateCategoryID = await _dbContext.Categories.AnyAsync(g => g.CategoryId == dto.CategoryId);

            if (!validateCategoryID)
                return BadRequest("Not validate Category ID");

            //var categoryId = _dbContext.subCategories.FindAsync(dto.CategoryId);


            subCategory sub = new subCategory
            {

               

                Name = dto.Name,

                Description = dto.Description,

                Image = stream.ToArray() ,

                 CategoryId = dto.CategoryId,
            };

            _dbContext.Add(sub);
            _dbContext.SaveChanges();
            return Ok(sub);
        }



        [HttpGet]

        public async Task<IActionResult> GetALLAsync()
        {

            var SubCategories = await _dbContext.subCategories.ToListAsync();

            return Ok(SubCategories);

        }


        [HttpGet("{id}")]

        public async Task<IActionResult> GetByIdLAsync(int id)
        {

            var subCategory = await _dbContext.subCategories.FindAsync(id);

            if (subCategory == null)
                return NotFound();

            return Ok(subCategory);

        }


        [HttpPut("{id}")]


        public async Task<IActionResult> updateAsync(int id, [FromForm] subCategoriesDto dto)
        {
            var subCategory = await _dbContext.subCategories.FindAsync(id);

            if (subCategory == null)
                return NotFound($"ID {id} not found");


            if (dto.Image != null)
            {


                if (!_allowedExtensions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                    return BadRequest("only png and jpg images are allowed");
                if (dto.Image.Length > _maxAllowedPosterSize)
                    return BadRequest("Max Allowed size for poster is 1 MB");

                using var stream = new MemoryStream();

                await dto.Image.CopyToAsync(stream);

                subCategory.Image = stream.ToArray();
            }


            subCategory.Name = dto.Name;
            subCategory.Description = dto.Description;

            bool validateCategoryID = await _dbContext.Categories.AnyAsync(g => g.CategoryId == dto.CategoryId);

            if (!validateCategoryID)
                return BadRequest("validate Category ID");

            subCategory.CategoryId = dto.CategoryId;
            await _dbContext.SaveChangesAsync();

            return Ok(subCategory);

        }

        [HttpDelete("{id}")]


        public async Task<IActionResult> deleteAsync(int id)
        {

            var subCategory = await _dbContext.subCategories.FindAsync(id);

            if (subCategory == null)
                return NotFound($"ID {id} not found");


            _dbContext.Remove(subCategory);
            _dbContext.SaveChanges();

            return Ok(subCategory);
        }
    }
}
