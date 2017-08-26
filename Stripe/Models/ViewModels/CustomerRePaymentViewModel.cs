using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stripe.Models
{
    public class CustomerRePaymentViewModel
    {
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Name*")]
        public string Name { get; set; }

        // Donation attributes
        public int DonationId { get; set; }
        public string CycleId { get; set; }

        public List<CustomerSubscriptionViewModel> Subscriptions { get; set; }

        // Address

        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; }

        [Display(Name = "Address Line 2")]
        public string AddressLine2 { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Zip")]
        public string Zip { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        public string Frequency { get; set; }

        public string Description { get; set; }

        public int Amount { get; set; }

        public string Last4Digit { get; set; }
        public string CardId { get; set; }

        [Required]
        public string Currency { get; set; }

        public string Paymentgatway { get; set; }

        public string DisableCurrencySelection { get; set; }
    }
}