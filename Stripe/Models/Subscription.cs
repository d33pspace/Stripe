using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stripe.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public string SubscriptionPlanId { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [MaxLength(50)]
        public string StripeId { get; set; }

        public string Status { get; set; }

        public int Amount { get; set; }

        public string Currency { get; set; }

        public string Interval { get; set; }

        public string Name { get; set; }
    }
}
