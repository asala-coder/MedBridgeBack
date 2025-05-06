﻿using MedBridge.Models;
using MedBridge.Models.ForgotPassword;
using MedBridge.Models.Messages;
using MedBridge.Models.Order;
using MedBridge.Models.ProductModels;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.models
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions <ApplicationDbContext > options) : base (options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ منع الحذف التتابعي بين `ProductModel` و `SubCategory`
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.SubCategory)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<subCategory>()
            .HasOne(s => s.Category)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete

            // Allow cascade delete for Category -> ProductModel relationship
            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete

            base.OnModelCreating(modelBuilder);

        
        }





        public DbSet<Category>Categories { get; set; }

        public DbSet<subCategory> subCategories { get; set; }

        public DbSet<ProductModel> Products { get; set; }

        public DbSet<User> users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<CartModel> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<ContactUs> ContactUs { get; set; }

        public DbSet<Favourite> Favourites { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Order> Orders { get; set; }

    }
}
