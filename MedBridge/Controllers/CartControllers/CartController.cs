using MedBridge.Dtos;
using MedBridge.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;
using System;
using System.Security.Claims;

namespace MedBridge.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // دالة لاستخراج UserId من الـ JWT
        private string GetUserId()
        {

            //return User.FindFirst("sub")?.Value;

            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] AddToCartDto model)
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)  
                    
                    return NotFound("Product not found.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == model.ProductId);
                if (cartItem == null)
                {
                    cartItem = new CartItem { ProductId = model.ProductId, Quantity = model.Quantity };
                    cart.CartItems.Add(cartItem);
                }
                else
                {
                    cartItem.Quantity += model.Quantity;
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Product added to cart", Cart = cart });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }
    }
}
