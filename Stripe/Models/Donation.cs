using System;

namespace Stripe.Models
{
    public class Donation
    {
        public int Id { get; set; }

        public string CycleId { get; set; }

        public double DonationAmount { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public DateTime? TransactionDate { get; set; }
    }
}