namespace ECommerce.Application.Addresses;

public record AddressDto(
    int Id, string FullName, string Phone, string Line1, string? Line2,
    string City, string Province, string? PostalCode, string Country, bool IsDefault);

public record AddressCreateUpdateRequest(
    string FullName, string Phone, string Line1, string? Line2,
    string City, string Province, string? PostalCode, string Country, bool IsDefault);
