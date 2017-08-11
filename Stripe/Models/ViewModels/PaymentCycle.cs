using System.ComponentModel;

namespace Stripe.Services
{
    public enum PaymentCycle
    {
        [Description("One Off")]
        OneOff = 1,
        [Description("Weekly")]
        Day = 2,
        [Description("Monthly")]
        Month = 3,
        [Description("Yearly")]
        Year = 4
    }
}