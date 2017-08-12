using System.ComponentModel;

namespace Stripe.Services
{
    public enum PaymentCycle
    {
        [Description("One Off")]
        OneOff = 1,
        [Description("Monthly")]
        Month = 2,
        [Description("Quarterly")]
        Quarter = 3,
        [Description("Yearly")]
        Year = 4
    }
}