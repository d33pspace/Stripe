using System.Collections.Generic;

namespace Stripe.Models
{
    public class CustomerPaymentViewModel
    {
        public string UserName { get; set; }
        public List<SubscriptionViewModel> Subscriptions { get; set; }
        public string CardNumber { get; set; }
        public string Cvc { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public int DonationId { get; set; }
        public string CycleId { get; set; }
    }
}