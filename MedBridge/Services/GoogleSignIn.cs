using Google.Apis.Auth;
using MedBridge.Models;
using MedBridge.Models.GoogLe_signIn;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static MedBridge.Services.Jwt_Services;

namespace MedBridge.Services
{
    public class GoogleSignIn : IGoogleSignIn
    {
        private readonly ApplicationDbContext _context;

        public GoogleSignIn(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GoogleLoginResponse> SignInWithGoogle(string googleToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
            var email = payload.Email;

            var user = _context.users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return new GoogleLoginResponse
                {
                    Status = "new_user",
                    Email = email
                };
            }
            else
            {
                var token = GenerateJwtToken(user);
                return new GoogleLoginResponse
                {
                    Status = "existing_user",
                    Email = user.Email,
                    Token = token
                };
            }
        }

        public async Task<bool> CompleteProfile(UserProfileRequest request)
        {
            var user = new User
            {

                Phone = request.Phone,
                Address = request.Address,
                MedicalSpecialist = request.MedicalSpecialist,
                CreatedAt = DateTime.UtcNow
            };

            _context.users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateJwtToken(User user)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("YOUR_SECRET_KEY");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }
}