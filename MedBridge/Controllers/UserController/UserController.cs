

using MedBridge.Models;
using MedBridge.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.models;
using Org.BouncyCastle.Crypto.Generators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MedBridge.Controllers
{
    [ApiController]
    [Route("api/MedBridge")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly IMemoryCache _memoryCache;

        public UserController(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<UserController> logger,
            IMemoryCache memoryCache)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpPost("User/signup")]
        public async Task<IActionResult> SignUp([FromForm] User user)
        {
            Console.WriteLine($"Name: {user.Name}, Email: {user.Email}, Password: {user.Password}, ConfirmPassword: {user.ConfirmPassword}");
            try
            {
                if (string.IsNullOrWhiteSpace(user.Password) ||
                    string.IsNullOrWhiteSpace(user.ConfirmPassword) || string.IsNullOrWhiteSpace(user.Name))
                {
                    return BadRequest("Invalid personal information.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    _logger.LogError("Model errors: " + string.Join(", ", errors)); // دي هتظهر في output
                    return BadRequest(new { errors });
                }


                if (user.Password != user.ConfirmPassword)
                {
                    return BadRequest("Password and Confirm Password do not match.");
                }
                var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    return BadRequest("Email already exists.");
                }

                PasswordHasher.CreatePasswordHash(user.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.Password = Convert.ToBase64String(passwordHash);
                user.PasswordSalt = passwordSalt;

                _context.users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered: {user.Email}");
                //_logger.LogInformation("User registered: {Email}", user.Email);
                return Ok("User registered successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SignUp");
                return StatusCode(500, new { error = "An error in SignUp." });
            }
        }

        [HttpPost("User/signin")]
        public async Task<IActionResult> SignIn([FromForm] User loginRequest)
        {
            try
            {
                var cacheEmail = $"LoginAttempts{loginRequest.Email}";
                var cackePassword = $"LoginAttempts{loginRequest.Password}";
                var attempts1 = _memoryCache.Get<int>(cackePassword);
                var attempts2 = _memoryCache.Get<int>(cacheEmail);

                if (attempts1 >= 5 || attempts2 >= 5)
                {
                    return BadRequest("Too many failed attempts. Try again after 15 minutes.");
                }

                var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
                if (existingUser == null)
                {
                    _memoryCache.Set(cacheEmail, attempts2 + 1, TimeSpan.FromMinutes(15));
                    return BadRequest("Invalid email or password.");
                }

                bool isPasswordValid = PasswordHasher.VerifyPasswordHash(
                    loginRequest.Password,
                    Convert.FromBase64String(existingUser.Password),
                    existingUser.PasswordSalt);

                if (!isPasswordValid)
                {
                    _memoryCache.Set(cacheEmail, attempts2 + 1, TimeSpan.FromMinutes(15));
                    return BadRequest("Invalid email or password.");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, existingUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, existingUser.Name),
                    new Claim(ClaimTypes.Email, existingUser.Email),
                    new Claim(ClaimTypes.Role, existingUser.Role)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                var newRefreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString(),
                    UserId = existingUser.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    Role = existingUser.Role
                };

                var oldTokens = _context.RefreshTokens.Where(rt => rt.UserId == existingUser.Id);
                _context.RefreshTokens.RemoveRange(oldTokens);
                _context.RefreshTokens.Add(newRefreshToken);
                await _context.SaveChangesAsync();

                _memoryCache.Remove(cacheEmail); // remove email old
                return Ok(new
                {
                    Token = tokenString,
                    ExpiresIn = 3600,
                    RefreshToken = newRefreshToken.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SignIn");
                return StatusCode(500, new { error = "An error in SignIn." });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] string refreshToken)
        {
            try
            {
                var existingToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (existingToken == null || existingToken.ExpiryDate <= DateTime.UtcNow)
                {
                    return Unauthorized("Invalid or expired refresh token.");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, existingToken.User.Id.ToString()),
                    new Claim(ClaimTypes.Name, existingToken.User.Name),
                    new Claim(ClaimTypes.Email, existingToken.User.Email),
                    new Claim(ClaimTypes.Role, existingToken.User.Role)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var newJwtToken = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials
                );

                var newTokenString = new JwtSecurityTokenHandler().WriteToken(newJwtToken);

                existingToken.Token = Guid.NewGuid().ToString();
                existingToken.ExpiryDate = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Token = newTokenString,
                    ExpiresIn = 3600,
                    RefreshToken = existingToken.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshToken");
                return StatusCode(500, new { error = "An error in RefreshToken ." });
            }
        }

        [HttpPost("User/logout")]
        public async Task<IActionResult> Logout([FromForm] string refreshToken)
        {
            var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (existingToken != null)
            {
                _context.RefreshTokens.Remove(existingToken);
                await _context.SaveChangesAsync();
            }
            return Ok("Logged out successfully.");
        }

    
        [HttpGet("User/{email}")]
        public async Task<IActionResult> GetUser(string email)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            return Ok(user);
        }

        [HttpPut("User/{Email}")]
        public async Task<IActionResult> UpdateUser(string Email, [FromForm] User updatedUser, IFormFile? profileImage)
        {
            if (Email != updatedUser.Email)
            {
                return BadRequest("User Email mismatch");
            }

            var existingUser = await _context.users.FirstOrDefaultAsync(u => u.Email == Email);

            if (existingUser == null)
            {
                return NotFound("User not found");
            }

            existingUser.Name = updatedUser.Name;
            existingUser.Email = updatedUser.Email;
            existingUser.MedicalSpecialist = updatedUser.MedicalSpecialist;

            // If password is provided, we need to hash it (or handle decoding/encryption as needed)
            if (!string.IsNullOrEmpty(updatedUser.Password))
            {
                existingUser.Password = HashPassword(updatedUser.Password);  // Hash the password before saving
            }

            // Optional: Handle the profile image
            if (profileImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await profileImage.CopyToAsync(memoryStream);
                    existingUser.ProfileImage = memoryStream.ToArray();
                }
            }

            _context.users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        private string HashPassword(string password)
        {
            // Implement your hashing logic here
            // For example, using bcrypt or any other hashing method
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        // In UserController:

        /// <summary>
        /// Partially updates a user’s Role and/or MedicalSpecialist.
        /// PATCH api/MedBridge/User/{email}
        /// </summary>
        [HttpPatch("User/info/{email}")]
        public async Task<IActionResult> UpdateUSerInfo(
            string email,
            [FromBody] RoleSpecialistUpdateDto dto)
        {
            // 1. Find existing user
            var existingUser = await _context.users
                .FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser == null)
                return NotFound(new { message = "User not found" });

            // 2. Update only the allowed fields
            if (!string.IsNullOrWhiteSpace(dto.Role))
                existingUser.Role = dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.MedicalSpecialist))
                existingUser.MedicalSpecialist = dto.MedicalSpecialist;

            // 3. Save
            _context.users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Role and MedicalSpecialist updated successfully" });
        }

        // DTO class 
        public class RoleSpecialistUpdateDto
        {
         
            public string? Role { get; set; }

  
            public string? MedicalSpecialist { get; set; }
        }


        [HttpDelete("User/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully" });
        }

    }
}
