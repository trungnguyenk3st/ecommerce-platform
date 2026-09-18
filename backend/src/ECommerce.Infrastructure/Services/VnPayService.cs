using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// VNPay "Payment URL" (pay) integration — https://sandbox.vnpayment.vn (sandbox docs).
/// Builds the redirect URL for checkout and validates the secure-hash signature VNPay
/// attaches to the browser return URL and to its server-to-server IPN callback.
/// </summary>
public class VnPayService : IVnPayService
{
    private readonly VnPayOptions _options;

    public VnPayService(IOptions<VnPayOptions> options)
    {
        _options = options.Value;
    }

    public string CreatePaymentUrl(Order order, string clientIpAddress)
    {
        var vnpParams = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = "2.1.0",
            ["vnp_Command"] = "pay",
            ["vnp_TmnCode"] = _options.TmnCode,
            ["vnp_Amount"] = ((long)(order.Total * 100)).ToString(),
            ["vnp_CurrCode"] = "VND",
            ["vnp_TxnRef"] = order.OrderNumber,
            ["vnp_OrderInfo"] = $"Payment for order {order.OrderNumber}",
            ["vnp_OrderType"] = "other",
            ["vnp_Locale"] = "vn",
            ["vnp_ReturnUrl"] = _options.ReturnUrl,
            ["vnp_IpAddr"] = string.IsNullOrWhiteSpace(clientIpAddress) ? "127.0.0.1" : clientIpAddress,
            ["vnp_CreateDate"] = DateTime.UtcNow.AddHours(7).ToString("yyyyMMddHHmmss"), // VNPay expects Asia/Ho_Chi_Minh (UTC+7)
            ["vnp_ExpireDate"] = DateTime.UtcNow.AddHours(7).AddMinutes(15).ToString("yyyyMMddHHmmss")
        };

        var (query, secureHash) = BuildSignedQuery(vnpParams);
        return $"{_options.BaseUrl}?{query}&vnp_SecureHash={secureHash}";
    }

    public VnPayCallbackResult ValidateCallback(IReadOnlyDictionary<string, string> queryParams)
    {
        var incomingHash = queryParams.GetValueOrDefault("vnp_SecureHash", string.Empty);

        var signable = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in queryParams)
        {
            if (key is "vnp_SecureHash" or "vnp_SecureHashType") continue;
            signable[key] = value;
        }

        var (_, computedHash) = BuildSignedQuery(signable);
        var isValidSignature = string.Equals(incomingHash, computedHash, StringComparison.OrdinalIgnoreCase);

        var responseCode = queryParams.GetValueOrDefault("vnp_ResponseCode", string.Empty);
        var orderNumber = queryParams.GetValueOrDefault("vnp_TxnRef", string.Empty);
        var transactionId = queryParams.GetValueOrDefault("vnp_TransactionNo", string.Empty);

        return new VnPayCallbackResult(isValidSignature, responseCode == "00", orderNumber, transactionId, responseCode);
    }

    private (string Query, string SecureHash) BuildSignedQuery(SortedDictionary<string, string> vnpParams)
    {
        var signData = string.Join('&', vnpParams
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));

        var hashBytes = Encoding.UTF8.GetBytes(signData);
        var keyBytes = Encoding.UTF8.GetBytes(_options.HashSecret);
        var hash = HMACSHA512.HashData(keyBytes, hashBytes);
        var secureHash = Convert.ToHexString(hash).ToLowerInvariant();

        return (signData, secureHash);
    }
}
