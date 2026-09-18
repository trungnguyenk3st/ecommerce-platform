namespace ECommerce.Application.Admin;

public record DashboardSummaryDto(
    int TotalProducts,
    int TotalCategories,
    int TotalCustomers,
    int TotalOrders,
    int PendingOrders,
    decimal TotalRevenue,
    decimal RevenueLast30Days,
    List<TopProductDto> TopProducts,
    List<RecentOrderDto> RecentOrders,
    List<LowStockProductDto> LowStockProducts);

public record TopProductDto(int ProductId, string Name, int UnitsSold, decimal Revenue);
public record RecentOrderDto(int Id, string OrderNumber, string CustomerEmail, decimal Total, string Status, DateTime CreatedAt);
public record LowStockProductDto(int ProductId, string Name, int StockQuantity);

public record CustomerDto(string Id, string Email, string FullName, IList<string> Roles, bool IsLocked);
