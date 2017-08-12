namespace Stripe.Models
{
    public class CustomerSubscriptionViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }
}