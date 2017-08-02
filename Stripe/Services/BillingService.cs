using System.Collections.Generic;

namespace Stripe.Services
{
    public class BillingService : IBillingService
    {
        public List<KeyValuePair<PaymentCycle, string>> GetCycles()
        {
            return EnumInfo<PaymentCycle>.GetValues();
        }
    }
}