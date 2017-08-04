using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Stripe.Models
{
    public class CreditCardViewModel
    {
        public int Id { get; set; }

        public string StripeId { get; set; }

        [NotMapped]
        public string StripeToken { get; set; }

        public string Name { get; set; }

        public string Last4 { get; set; }

        public string CardType { get; set; }

        public List<SelectListItem> CardTypes { get; set; }

        public string AddressCity { get; set; }

        public string AddressZip { get; set; }

        public string AddressCountry { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        [MaxLength(16)]
        [NotMapped]
        public string CardNumber { get; set; }

        public string Cvc { get; set; }

        public int? ExpirationMonth { get; set; }

        public int? ExpirationYear { get; set; }

        public string CardCountry { get; set; }

        public string UserId { get; set; }

        public void ClearCreditCardDetails()
        {
            ExpirationMonth = null;
            ExpirationYear = null;
            Last4 = null;
            StripeId = null;
            StripeToken = null;
            Cvc = null;
            CardType = null;
            CardTypes = new List<SelectListItem>();
        }
    }

    public class PaymentViewViewModel
    {
        public string UserId { get; set; }

        public List<SubscriptionViewModel> Subscriptions { get; set; }

        public CreditCardViewModel CreditCard { get; set; }

        public PaymentViewViewModel()
        {
            Subscriptions = new List<SubscriptionViewModel>();
        }
    }

    public class SubscriptionViewModel
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

    }
}