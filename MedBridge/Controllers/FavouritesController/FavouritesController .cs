using MedBridge.Dtos;
using MedBridge.DTOs;
using MedBridge.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;
using System.Security.Claims;

namespace MedBridge.Controllers
{
    [Route("api/favourites")]
    [ApiController]
    public class FavouritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavouritesController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        
        [HttpPost("add")]
        public async Task<IActionResult> AddToFavourites([FromBody] AddToFavouritesDto model)
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                    return NotFound("Product not found.");

                var existing = await _context.Favourites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == model.ProductId);

                if (existing != null)
                    return BadRequest("Product already in favourites.");

                var favourite = new Favourite
                {
                    UserId = userId,
                    ProductId = model.ProductId
                };

                _context.Favourites.Add(favourite);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Product added to favourites", Favourite = favourite });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

       
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromFavourites(int productId)
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var favourite = await _context.Favourites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                if (favourite == null)
                    return NotFound("Product not in favourites.");

                _context.Favourites.Remove(favourite);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Product removed from favourites" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUserFavourites()
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var favourites = await _context.Favourites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Product)
                    .ToListAsync();

                return Ok(favourites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }
    }
}
