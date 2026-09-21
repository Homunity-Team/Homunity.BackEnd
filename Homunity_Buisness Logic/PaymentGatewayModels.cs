using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // طلب الشحن المُرسَل لبوابة الدفع — منفصل عمدًا عن MockProcessRequest (طلب العميل للـAPI)
    // لأن هذا هو الشكل الداخلي الذي تحتاجه أي بوابة دفع فعلية لاحقًا، بغض النظر عن شكل طلب الـHTTP الخارجي.
    public class PaymentGatewayRequest
    {
        public string OrderId { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardCvv { get; set; }
        public decimal Amount { get; set; }
    }

    public enum PaymentGatewayResultStatus { Approved, Declined, Error }

    // نتيجة الشحن من البوابة — منفصلة عن PaymentStatusResponse (حالة الدفع كما تُعرَض للعميل عبر الـAPI)
    public class PaymentGatewayResult
    {
        public bool Success { get; set; }
        public PaymentGatewayResultStatus Status { get; set; }
        public string? ProviderTransactionId { get; set; }
        public string? FailureReason { get; set; }

        public static PaymentGatewayResult Approved(string transactionId) =>
            new() { Success = true, Status = PaymentGatewayResultStatus.Approved, ProviderTransactionId = transactionId };

        public static PaymentGatewayResult Declined(string reason) =>
            new() { Success = false, Status = PaymentGatewayResultStatus.Declined, FailureReason = reason };

        public static PaymentGatewayResult Error(string reason) =>
            new() { Success = false, Status = PaymentGatewayResultStatus.Error, FailureReason = reason };
    }
}
