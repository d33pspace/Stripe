using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stripe.Models
{
    public class CustomerPaymentViewModel
    {
        public string UserName { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [CreditCard]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"\d{3}", ErrorMessage = "Invalid CVC number")]
        public string Cvc { get; set; }

        [Range(1, 12, ErrorMessage = "Invalid month")]
        public int ExpiryMonth { get; set; }

        [Range(17, 30, ErrorMessage = "Invalid year")]
        public int ExpiryYear { get; set; }
        
        // Donation attributes
        public int DonationId { get; set; }
        public string CycleId { get; set; }

        public List<CustomerSubscriptionViewModel> Subscriptions { get; set; }

    }
}