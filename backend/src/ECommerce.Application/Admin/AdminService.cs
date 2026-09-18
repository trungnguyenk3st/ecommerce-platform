using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Admin;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identity;

    public AdminService(IApplicationDbContext db, IIdentityService identity)
    {
        _db = db;
        _identity = identity;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var revenueStatuses = new[] { OrderStatus.Paid, OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered };
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalProducts = await _db.Products.CountAsync();
        var totalCategories = await _db.Categories.CountAsync();
        var totalOrders = await _db.Orders.CountAsync();
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.PendingPayment || o.Status == OrderStatus.Processing);
        var customers = await _identity.GetAllCustomersAsync();

        var totalRevenue = await _db.Orders.Where(o => revenueStatuses.Contains(o.Status)).SumAsync(o => (decimal?)o.Total) ?? 0;
        var revenueLast30Days = await _db.Orders
            .Where(o => revenueStatuses.Contains(o.Status) && o.CreatedAt >= thirtyDaysAgo)
            .SumAsync(o => (decimal?)o.Total) ?? 0;

        var topProducts = await _db.OrderItems
            .Where(oi => revenueStatuses.Contains(oi.Order.Status))
            .GroupBy(oi => new { oi.ProductId, oi.ProductName })
            .Select(g => new TopProductDto(g.Key.ProductId, g.Key.ProductName, g.Sum(x => x.Quantity), g.Sum(x => x.UnitPrice * x.Quantity)))
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        var recentOrdersRaw = await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new { o.Id, o.OrderNumber, o.UserId, o.Total, o.Status, o.CreatedAt })
            .ToListAsync();

        var recentOrders = new List<RecentOrderDto>();
        foreach (var o in recentOrdersRaw)
        {
            var user = await _identity.GetUserAsync(o.UserId);
            recentOrders.Add(new RecentOrderDto(o.Id, o.OrderNumber, user?.Email ?? "unknown", o.Total, o.Status.ToString(), o.CreatedAt));
        }

        var lowStockProducts = await _db.Products
            .Where(p => p.IsActive && p.StockQuantity <= 5)
            .OrderBy(p => p.StockQuantity)
            .Take(10)
            .Select(p => new LowStockProductDto(p.Id, p.Name, p.StockQuantity))
            .ToListAsync();

        return new DashboardSummaryDto(
            totalProducts, totalCategories, customers.Count, totalOrders, pendingOrders,
            totalRevenue, revenueLast30Days, topProducts, recentOrders, lowStockProducts);
    }

    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        var customers = await _identity.GetAllCustomersAsync();
        return customers.Select(c => new CustomerDto(c.Id, c.Email, c.FullName, c.Roles, c.IsLocked)).ToList();
    }

    public async Task<CustomerDto> SetCustomerLockAsync(string userId, bool locked)
    {
        var success = await _identity.SetLockoutAsync(userId, locked);
        if (!success) throw new NotFoundException("User", userId);

        var user = await _identity.GetUserAsync(userId) ?? throw new NotFoundException("User", userId);
        return new CustomerDto(user.Id, user.Email, user.FullName, user.Roles, user.IsLocked);
    }
}
