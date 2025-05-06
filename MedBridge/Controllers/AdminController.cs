using MedBridge.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.models;
using static MedBridge.Models.User;

namespace MedBridge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        
            private readonly ApplicationDbContext _context;

            public AdminController(ApplicationDbContext context)
            {
                _context = context;
            }

            // Add Admin
            [HttpPost("add-admin")]
            public async Task<IActionResult> AddAdmin(string email, string password)
            {
                if (await _context.users.AnyAsync(u => u.Email == email))
                    return BadRequest("Admin already exists.");

                var admin = new User
                {
                    Email = email,
                    Password = password,
                    Role = "Admin"
                };

                _context.users.Add(admin);
                await _context.SaveChangesAsync();
                return Ok("Admin added.");
            }

            // Delete Admin (except main admin)
            [HttpDelete("delete-admin/{id}")]
            public async Task<IActionResult> DeleteAdmin(int id)
            {
                var admin = await _context.users.FindAsync(id);
                if (admin == null || admin.Role != "Admin")
                    return NotFound("Admin not found.");

                if (admin.Id == 1)
                    return BadRequest("Main admin cannot be deleted.");

                _context.users.Remove(admin);
                await _context.SaveChangesAsync();
                return Ok("Admin deleted.");
            }

            // Block User
            [HttpPost("block-user/{id}")]
            public async Task<IActionResult> BlockUser(int id)
            {
                var user = await _context.users.FindAsync(id);
                if (user == null || user.Role == "Admin")
                    return BadRequest("Invalid user.");

                user.Status = UserStatus.Blocked;
                await _context.SaveChangesAsync();
                return Ok("User blocked.");
            }

            // Deactivate User
            [HttpPost("deactivate-user/{id}")]
            public async Task<IActionResult> DeactivateUser(int id)
            {
                var user = await _context.users.FindAsync(id);
                if (user == null || user.Role == "Admin")
                    return BadRequest("Invalid user.");

                user.Status = UserStatus.Deactivated;
                await _context.SaveChangesAsync();
                return Ok("User deactivated.");
            }
        }
    }

