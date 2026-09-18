using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Addresses;

public class AddressService : IAddressService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<AddressCreateUpdateRequest> _validator;

    public AddressService(IApplicationDbContext db, IValidator<AddressCreateUpdateRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<List<AddressDto>> GetAllAsync(string userId)
    {
        var addresses = await _db.Addresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
        return addresses.Select(ToDto).ToList();
    }

    public async Task<AddressDto> CreateAsync(string userId, AddressCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        if (request.IsDefault)
            await ClearExistingDefaultAsync(userId);

        var address = new Address
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            Line1 = request.Line1,
            Line2 = request.Line2,
            City = request.City,
            Province = request.Province,
            PostalCode = request.PostalCode,
            Country = request.Country,
            IsDefault = request.IsDefault
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();
        return ToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(string userId, int id, AddressCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
            ?? throw new NotFoundException(nameof(Address), id);

        if (request.IsDefault && !address.IsDefault)
            await ClearExistingDefaultAsync(userId);

        address.FullName = request.FullName;
        address.Phone = request.Phone;
        address.Line1 = request.Line1;
        address.Line2 = request.Line2;
        address.City = request.City;
        address.Province = request.Province;
        address.PostalCode = request.PostalCode;
        address.Country = request.Country;
        address.IsDefault = request.IsDefault;
        address.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(address);
    }

    public async Task DeleteAsync(string userId, int id)
    {
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId)
            ?? throw new NotFoundException(nameof(Address), id);

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
    }

    private async Task ClearExistingDefaultAsync(string userId)
    {
        var current = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
        foreach (var a in current) a.IsDefault = false;
    }

    private static AddressDto ToDto(Address a) => new(
        a.Id, a.FullName, a.Phone, a.Line1, a.Line2, a.City, a.Province, a.PostalCode, a.Country, a.IsDefault);
}
