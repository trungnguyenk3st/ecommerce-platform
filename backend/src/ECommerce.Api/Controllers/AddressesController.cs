using ECommerce.Api.Common;
using ECommerce.Application.Addresses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
public class AddressesController : ApiControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AddressDto>>> GetAll() => Ok(await _addressService.GetAllAsync(CurrentUserId));

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create(AddressCreateUpdateRequest request)
        => Ok(await _addressService.CreateAsync(CurrentUserId, request));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AddressDto>> Update(int id, AddressCreateUpdateRequest request)
        => Ok(await _addressService.UpdateAsync(CurrentUserId, id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _addressService.DeleteAsync(CurrentUserId, id);
        return NoContent();
    }
}
