using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stripe.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }

        public DateTime? TrialStart { get; set; }

        public DateTime? TrialEnd { get; set; }

        public string SubscriptionPlanId { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [MaxLength(50)]
        public string StripeId { get; set; }

        public string Status { get; set; }

        public decimal TaxPercent { get; set; }

        public string ReasonToCancel { get; set; }

        public bool IsTrialing()
        {
            return TrialStart != null && TrialEnd != null && TrialEnd > DateTime.UtcNow;
        }
        private bool IsTerminated()
        {
            return this.End != null && this.End < DateTime.UtcNow;
        }
    }
}
