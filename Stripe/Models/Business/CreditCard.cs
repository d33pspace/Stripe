using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stripe.Models
{
    public class CreditCard
    {
        public int Id { get; set; }

        public string StripeId { get; set; }

        [NotMapped]
        public string StripeToken { get; set; }

        public string Name { get; set; }

        public string Last4 { get; set; }

        public string Type { get; set; }

        public string Fingerprint { get; set; }

        public string AddressCity { get; set; }

        public string AddressCountry { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressState { get; set; }

        public string AddressZip { get; set; }

        [MaxLength(16)]
        [NotMapped]
        public string CardNumber { get; set; }

        public string Cvc { get; set; }

        public string ExpirationMonth { get; set; }

        public string ExpirationYear { get; set; }

        public string CardCountry { get; set; }

        public string SaasEcomUserId { get; set; }

        public void ClearCreditCardDetails()
        {
            this.ExpirationMonth = null;
            this.ExpirationYear = null;
            this.Last4 = null;
            this.Fingerprint = null;
            this.StripeId = null;
            this.StripeToken = null;
            this.Cvc = null;
            this.Type = null;
        }
    }
}
