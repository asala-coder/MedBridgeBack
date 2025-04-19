using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedBridge.Models.ProductModels
{
    public class ProductModel
    {
        [Key]
        public int ProductId { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsNew { get; set; }

        public double Discount { get; set; }

        // Foreign key for SubCategory
        public int SubCategoryId { get; set; }
        public subCategory SubCategory { get; set; }

        // ✅ Add Foreign key for Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<string> ImageUrls { get; set; }
        public int UserId { get; set; }
        public User user { get; set; }
    }
}
