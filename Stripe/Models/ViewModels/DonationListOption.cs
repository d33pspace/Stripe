namespace Stripe.Models
{
    public class DonationListOption
    {
        public int Id { get; set; }

        public int Amount { get; set; }

        public string Reason { get; set; }

        public bool IsCustom { get; set; }
    }
}