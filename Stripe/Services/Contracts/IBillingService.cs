using System.Collections.Generic;

namespace Stripe.Services
{
    public interface IBillingService
    {
        List<KeyValuePair<PaymentCycle, string>> GetCycles();
    }
}