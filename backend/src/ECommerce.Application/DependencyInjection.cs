using System.Reflection;
using ECommerce.Application.Addresses;
using ECommerce.Application.Admin;
using ECommerce.Application.Auth;
using ECommerce.Application.Carts;
using ECommerce.Application.Categories;
using ECommerce.Application.Coupons;
using ECommerce.Application.Orders;
using ECommerce.Application.Products;
using ECommerce.Application.Reviews;
using ECommerce.Application.Wishlist;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
