using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stripe
{
    public class BillingSettings
    {
        public virtual string StripeApiKey { get; set; }

        public virtual string StripeWebhookKey { get; set; }
    }
}
