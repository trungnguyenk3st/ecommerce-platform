namespace ECommerce.Application.Addresses;

public interface IAddressService
{
    Task<List<AddressDto>> GetAllAsync(string userId);
    Task<AddressDto> CreateAsync(string userId, AddressCreateUpdateRequest request);
    Task<AddressDto> UpdateAsync(string userId, int id, AddressCreateUpdateRequest request);
    Task DeleteAsync(string userId, int id);
}
