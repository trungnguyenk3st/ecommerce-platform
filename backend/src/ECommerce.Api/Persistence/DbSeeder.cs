using ECommerce.Application.Common;
using ECommerce.Application.Common.Constants;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Identity;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Persistence;

/// <summary>Seeds roles, a demo admin account, and a small catalog so the app is explorable right after first run.</summary>
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        const string adminEmail = "admin@ecommerce.local";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Store Administrator", EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, "Admin@12345");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, Roles.Admin);
        }

        if (await db.Categories.AnyAsync()) return; // catalog already seeded

        var electronics = new Category { Name = "Electronics", Slug = Slugifier.Slugify("Electronics"), DisplayOrder = 1 };
        var phones = new Category { Name = "Phones", Slug = Slugifier.Slugify("Phones"), DisplayOrder = 1, ParentCategory = electronics };
        var laptops = new Category { Name = "Laptops", Slug = Slugifier.Slugify("Laptops"), DisplayOrder = 2, ParentCategory = electronics };
        var fashion = new Category { Name = "Fashion", Slug = Slugifier.Slugify("Fashion"), DisplayOrder = 2 };
        var mensWear = new Category { Name = "Men's Wear", Slug = Slugifier.Slugify("Men's Wear"), DisplayOrder = 1, ParentCategory = fashion };
        var homeGoods = new Category { Name = "Home & Living", Slug = Slugifier.Slugify("Home & Living"), DisplayOrder = 3 };

        db.Categories.AddRange(electronics, phones, laptops, fashion, mensWear, homeGoods);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            NewProduct("Aurora X1 Smartphone", "AUR-X1-128", 12_990_000m, 14_990_000m, 42, phones.Id,
                "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=800"),
            NewProduct("Nimbus Pro 14 Laptop", "NIM-P14-512", 27_500_000m, null, 15, laptops.Id,
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=800"),
            NewProduct("Streamline Wireless Earbuds", "STR-EB-01", 1_490_000m, 1_890_000m, 120, phones.Id,
                "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?w=800"),
            NewProduct("Classic Oxford Shirt", "CLS-OXF-M", 590_000m, null, 80, mensWear.Id,
                "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=800"),
            NewProduct("Tailored Chino Trousers", "TLR-CHN-32", 690_000m, 790_000m, 60, mensWear.Id,
                "https://images.unsplash.com/photo-1473966968600-fa801b869a1a?w=800"),
            NewProduct("Ceramic Pour-Over Coffee Set", "CER-PC-01", 450_000m, null, 30, homeGoods.Id,
                "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?w=800"),
            NewProduct("Linen Throw Blanket", "LIN-TB-02", 320_000m, 390_000m, 0, homeGoods.Id,
                "https://images.unsplash.com/photo-1584100936595-c0654b55a2e2?w=800"),
            NewProduct("Nimbus Air Gaming Mouse", "NIM-GM-07", 890_000m, null, 55, phones.Id,
                "https://images.unsplash.com/photo-1527814050087-3793815479db?w=800"),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        db.Coupons.Add(new Coupon
        {
            Code = "WELCOME10",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            MinOrderAmount = 500_000m,
            MaxDiscountAmount = 300_000m,
            MaxUsageCount = 500,
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            IsActive = true
        });
        await db.SaveChangesAsync();
    }

    private static Product NewProduct(string name, string sku, decimal price, decimal? compareAt, int stock, int categoryId, string imageUrl) => new()
    {
        Name = name,
        Slug = Slugifier.Slugify(name),
        Description = $"{name} — a top pick from our catalog, built for everyday reliability and great value.",
        Sku = sku,
        Price = price,
        CompareAtPrice = compareAt,
        StockQuantity = stock,
        IsActive = true,
        CategoryId = categoryId,
        Images = new List<ProductImage> { new() { Url = imageUrl, DisplayOrder = 0, IsPrimary = true } }
    };
}
