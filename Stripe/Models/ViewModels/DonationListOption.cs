namespace Stripe.Models
{
    public class DonationListOption
    {
        public int Id { get; set; }

        public double Amount { get; set; }

        public string Reason { get; set; }

        public string Description => $"{Amount} {Reason}";

        public bool IsCustom { get; set; }
    }
}