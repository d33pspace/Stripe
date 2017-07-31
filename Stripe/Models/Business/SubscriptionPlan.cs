using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Stripe.Models
{
    public class SubscriptionPlan
    {
        public SubscriptionPlan()
        {
        }

        [Display(Name = "Plan Identifier")]
        [Required(ErrorMessage = "Please set a plan identifier.")]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0.0, 1000000)]
        public double Price { get; set; }

        public string Currency { get; set; }

        [Required]
        public SubscriptionInterval Interval { get; set; }

        [Display(Name = "Trial period in days")]
        [Range(0, 365)]
        public int TrialPeriodInDays { get; set; }

        public bool Disabled { get; set; }

        
    }

    public enum SubscriptionInterval
    {
        Monthly = 1,
        Yearly = 2,
        Weekly = 3,
        EverySixMonths = 4,
        EveryThreeMonths = 5
    }
}