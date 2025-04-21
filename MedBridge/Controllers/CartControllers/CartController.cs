using MedBridge.Dtos;
using MedBridge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedBridge.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public CartController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromForm] CartItemDto model)
        {
            try
            {
                if (model.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                    return NotFound("Product not found.");

                if (product.StockQuantity <= 0)
                    return BadRequest("Product is out of stock.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new CartModel { UserId = userId, CartItems = new List<CartItem>() };
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

                var serializedCart = _cartService.SerializeCart(cart);

                return Ok(new { Message = "Product added to cart", Cart = serializedCart });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding product to cart.", Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                    return Ok(new { Message = "Cart is empty" });

                var serializedCart = _cartService.SerializeCart(cart);

                return Ok(serializedCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching cart.", Error = ex.Message });
            }
        }

        [HttpDelete("delete/{productId}")]
        public async Task<IActionResult> DeleteFromCart(int productId)
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return NotFound("Cart not found.");

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (cartItem == null)
                    return NotFound("Product not found in cart.");

                cart.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                var serializedCart = _cartService.SerializeCart(cart );

                return Ok(new { Message = "Product removed from cart", Cart = serializedCart });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting product from cart.", Error = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemDto model)
        {
            try
            {
                Console.WriteLine($"ProductId: {model.ProductId}, Quantity: {model.Quantity}");

                // Validate quantity
                if (model.Quantity <= 0)
                    return BadRequest("Quantity must be greater than 0.");

                // Get the user ID (assuming GetUserId is a method that retrieves the current user's ID)
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                // Retrieve the cart for the user
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // If no cart is found, return NotFound
                if (cart == null)
                    return NotFound("Cart not found.");

                // Find the cart item to update
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == model.ProductId);
                if (cartItem == null)
                    return NotFound("Product not found in cart.");

                // Update the quantity
                cartItem.Quantity = model.Quantity;
                await _context.SaveChangesAsync();

                // Serialize the updated cart
                var serializedCart = _cartService.SerializeCart(cart);

                // Return a success message and the updated cart
                return Ok(new { Message = "Quantity updated", Cart = serializedCart });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");

                // Return a generic error message
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }



        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                string userId = GetUserId();
                if (userId == null)
                    return Unauthorized("Invalid user.");

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                    return NotFound("Cart not found.");

                cart.CartItems.Clear();
                await _context.SaveChangesAsync();

                var serializedCart = _cartService.SerializeCart(cart);

                return Ok(new { Message = "Cart cleared", Cart = serializedCart });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while clearing cart.", Error = ex.Message });
            }
        }
    }
}
