using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stripe.Models.Business
{
    public class OrganizationBilling
    {
        public Source PaymentSource { get; set; }
        public StripeSubscription Subscription { get; set; }
        public StripeInvoice UpcomingInvoice { get; set; }
        public IEnumerable<StripeCharge> Charges { get; set; } = new List<StripeCharge>();
    }
}
