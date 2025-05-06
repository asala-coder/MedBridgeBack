﻿using System.ComponentModel.DataAnnotations.Schema;
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

        public int StockQuantity { get; set; }

        public bool IsNew { get; set; }

        public double Discount { get; set; }

        // Foreign key for SubCategory
        public int SubCategoryId { get; set; }
        public subCategory SubCategory { get; set; }

        // Foreign key for Category
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Store image URLs
        public List<string> ImageUrls { get; set; } = new List<string>();

        // Foreign key for User
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
