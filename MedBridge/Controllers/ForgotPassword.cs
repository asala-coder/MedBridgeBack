using MedBridge.Models.ForgotPassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.models;

namespace MedBridge.Controllers
{
    [Route("api/ForgotPassword")]
    [ApiController]
    public class ForgotPassword : ControllerBase
    {
            private readonly ApplicationDbContext _context;

            public ForgotPassword(ApplicationDbContext context)
            {
                _context = context;
            }

            [HttpPost("request-reset")]
            public IActionResult RequestReset([FromBody] ForgotPasswordRequest request)
            {
                var user = _context.users.FirstOrDefault(u => u.Email == request.Email);
                if (user == null)
                    return NotFound("Email is not registered");

                var token = Guid.NewGuid().ToString();
                var expiry = DateTime.Now.AddHours(1);

                var resetToken = new PasswordResetToken
                {
                    Email = request.Email,
                    Token = token,
                    ExpiryDate = expiry
                };

                _context.PasswordResetTokens.Add(resetToken);
                _context.SaveChanges();

                // Ideally, you should send an email here with the token/link
                return Ok(new { Message = "Password reset link has been sent to your email", Token = token });
            }

            [HttpPost("reset-password")]
            public IActionResult ResetPassword([FromBody] ResetPassword request)
            {
                var tokenRecord = _context.PasswordResetTokens.FirstOrDefault(t => t.Token == request.Token);

                if (tokenRecord == null || tokenRecord.ExpiryDate < DateTime.Now)
                    return BadRequest("Token is invalid or has expired");

                var user = _context.users.FirstOrDefault(u => u.Email == tokenRecord.Email);
                if (user == null)
                    return NotFound("User not found");

                // NOTE: Hash the password if you're using hashing
                user.Password = request.NewPassword;

                _context.PasswordResetTokens.Remove(tokenRecord);
                _context.SaveChanges();

                return Ok("Password has been reset successfully");
            }
        }
    }


