using System.ComponentModel;

namespace Stripe.Services
{
    public enum PaymentCycle
    {
        [Description("Monthly")]
        Month = 1,
        [Description("Quarterly")]
        Quarter = 2,
        [Description("Yearly")]
        Year = 3,
        [Description("One Off")]
        OneOff = 4
    }
}