namespace ECommerce.Application.Admin;

public interface IAdminService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
    Task<List<CustomerDto>> GetCustomersAsync();
    Task<CustomerDto> SetCustomerLockAsync(string userId, bool locked);
}
