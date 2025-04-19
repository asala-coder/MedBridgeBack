using MedBridge.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MedBridge.Dtos.Product
{
    public class ProductDto
    {
       
        public int ProductId { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; }

       
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsNew { get; set; }

        public double Discount { get; set; }

        
        public int SubCategoryId { get; set; }
   
       
        public int CategoryId { get; set; }
        public List<IFormFile> Images { get; set; } // Multiple images
        public int UserId { get; set; }

        //public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
