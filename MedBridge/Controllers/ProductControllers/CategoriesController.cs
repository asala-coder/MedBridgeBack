using MedBridge.Models.ProductModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;

namespace MedBridge.Controllers.ProductControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;
        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private double _maxAllowedPosterSize = 1048576;

        public CategoriesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CategoryDto dto)
        {

            if (dto.Image == null)
                return BadRequest("image is required");
            if (!_allowedExtensions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                return BadRequest("only png and jpg images are allowed");
            if (dto.Image.Length > _maxAllowedPosterSize)
                return BadRequest("Max Allowed size for image is 10 MB");

            using var stream = new MemoryStream();

            await dto.Image.CopyToAsync(stream);


            Category category = new Category
            {

                CategoryId = dto.CategoryId,

                Name = dto.Name,

                Description = dto.Description,

                Image = stream.ToArray()
            };

            _dbContext.Add(category);
            _dbContext.SaveChanges();
            return Ok(category);
        }



        [HttpGet]

        public async Task<IActionResult> GetALLAsync()
        {

            var categories = await _dbContext.Categories.ToListAsync();

            return Ok(categories);

        }


        [HttpGet("{id}")]

        public async Task<IActionResult> GetByIdLAsync(int id)
        {

            var category = await _dbContext.Categories.FindAsync(id);

            if (category == null)
                return NotFound();

            return Ok(category);

        }


        [HttpPut("{id}")]


        public async Task <IActionResult> updateAsync(int id, [FromForm] CategoryDto  dto)
        {
            var category = await _dbContext.Categories.FindAsync(id);

            if (category == null)
                return NotFound($"ID {id} not found");


            if (dto. Image!= null)
            {


                if (!_allowedExtensions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                    return BadRequest("only png and jpg images are allowed");
                if (dto.Image.Length > _maxAllowedPosterSize)
                    return BadRequest("Max Allowed size for poster is 1 MB");

                using var stream = new MemoryStream();

                await dto.Image.CopyToAsync(stream);

                category.Image = stream.ToArray();
            }


            category.Name = dto.Name;
            category.Description = dto.Description;
            _dbContext.SaveChanges();
            return Ok(category);

        }

        [HttpDelete("{id}")]


        public async Task <IActionResult> deleteAsync(int id)
        {

            var category = await _dbContext.Categories.FindAsync(id);

            if (category == null)
                return NotFound($"ID {id} not found");


            _dbContext.Remove(category);
            _dbContext.SaveChanges();

            return Ok(category);
        }
    }
}
