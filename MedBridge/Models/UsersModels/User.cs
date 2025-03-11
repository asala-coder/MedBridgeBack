
using MedBridge.Models.Messages;
using MedBridge.Models.ProductModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedBridge.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50)]
        public string? Name { get; set; }
        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "The Email address is not valid.")]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "The password must contain at least 8 number, one letter, and one special character.")]
        public string? Password { get; set; }
        public byte[]? PasswordSalt { get; set; }

        [NotMapped]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "The password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "The password must contain at least 8 number, one letter, and one special character.")]
        public string? ConfirmPassword { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

       // [Range(1000000000, 9999999999, ErrorMessage = "Invalid phone number.")]
        public long Phone { get; set; }
        public string? MedicalSpecialist { get; set; }
        public string? Address { get; set; }
        public byte[]? ProfileImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "User";


        public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();

        public ICollection<ContactUs> ContactUs { get; set; } = new List<ContactUs>();
    }
}
