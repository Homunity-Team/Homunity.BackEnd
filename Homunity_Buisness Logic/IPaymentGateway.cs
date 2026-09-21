using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homunity_Buisness_Logic
{
    // التجريد الذي يمثّل أي مزوّد دفع (حالي أو مستقبلي). PaymentService لا يعرف
    // أي تفاصيل عن كيفية تنفيذ الشحن فعليًا — هذا بالضبط الهدف من Sprint 5 Part A.
    public interface IPaymentGateway
    {
        Task<PaymentGatewayResult> ChargeAsync(PaymentGatewayRequest request);
    }

}
