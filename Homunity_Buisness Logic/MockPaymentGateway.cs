using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // التنفيذ الوهمي الحالي — منقول بالكامل من داخل PaymentService القديم (Sprint <5)
    // بدون أي تغيير في منطق التحقق من الكارت (نفس الأرقام التجريبية بالضبط).
    // استبدال هذا الكلاس بمزوّد حقيقي لاحقًا لا يتطلب لمس PaymentService على الإطلاق.
    public class MockPaymentGateway : IPaymentGateway
    {
        private static readonly string[] ValidCards =
        {
            "4111111111111111",
            "5500005555555559",
            "4000000000000002"
        };

        private readonly ILogger<MockPaymentGateway> _logger;
        public MockPaymentGateway(ILogger<MockPaymentGateway> logger) => _logger = logger;

        public Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.CardNumber))
                {
                    _logger.LogWarning("MockPaymentGateway: charge rejected for order {OrderId} — missing card details.", request?.OrderId);
                    return Task.FromResult(PaymentGatewayResult.Declined("Invalid card"));
                }

                string cleanCard = request.CardNumber.Replace(" ", "");
                bool cardValid = ValidCards.Contains(cleanCard);

                // لا يتم تسجيل رقم الكارت كاملًا أبدًا — فقط آخر 4 أرقام لأغراض التتبع.
                _logger.LogInformation("MockPaymentGateway: charge attempt for order {OrderId}, card ending {Last4}.",
                    request.OrderId, MaskCard(cleanCard));

                if (!cardValid)
                {
                    _logger.LogWarning("MockPaymentGateway: charge declined for order {OrderId} — invalid card.", request.OrderId);
                    return Task.FromResult(PaymentGatewayResult.Declined("Invalid card"));
                }

                var transactionId = $"MOCK-TXN-{Guid.NewGuid():N}";
                _logger.LogInformation("MockPaymentGateway: charge approved for order {OrderId}, transaction {TransactionId}.",
                    request.OrderId, transactionId);

                return Task.FromResult(PaymentGatewayResult.Approved(transactionId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MockPaymentGateway: unexpected error while charging order {OrderId}.", request?.OrderId);
                return Task.FromResult(PaymentGatewayResult.Error("Payment gateway error"));
            }
        }

        private static string MaskCard(string cardNumber) =>
            string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 4 ? "****" : $"****{cardNumber[^4..]}";
    }

}
