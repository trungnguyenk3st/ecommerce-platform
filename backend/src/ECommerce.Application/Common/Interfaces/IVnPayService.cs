using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

public record VnPayCallbackResult(bool IsValidSignature, bool IsSuccess, string OrderNumber, string TransactionId, string ResponseCode);

public interface IVnPayService
{
    /// <summary>Builds the VNPay-hosted checkout URL the browser should redirect to for the given order.</summary>
    string CreatePaymentUrl(Order order, string clientIpAddress);

    /// <summary>Verifies the secure-hash signature on a VNPay return/IPN query string and extracts the result.</summary>
    VnPayCallbackResult ValidateCallback(IReadOnlyDictionary<string, string> queryParams);
}
