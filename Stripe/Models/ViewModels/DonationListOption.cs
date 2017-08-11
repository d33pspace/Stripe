namespace Stripe.Models
{
    public class DonationListOption
    {
        public int Id { get; set; }

        public int Amount { get; set; }

        public string Reason { get; set; }

        //public string Description => $"{Amount} {Reason}";

        public bool IsCustom { get; set; }
    }
}