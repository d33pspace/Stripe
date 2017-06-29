using Stripe;

namespace Stripe.Data
{
    public class ChargeProvider : IChargeProvider
    {
        private readonly StripeChargeService _chargeService;

        public ChargeProvider()
        {
            _chargeService = new StripeChargeService("pk_test_W9r8jIofygDYmsy7gUcjrVEG");
        }

        public bool CreateCharge(int amount, string currency, string description, string customerId, out string error)
        {
            var options = new StripeChargeCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Description = description
            };

            if (!string.IsNullOrEmpty(customerId))
            {
                options.CustomerId = customerId;
            }

            var result = _chargeService.Create(options);

            if (result.Captured != null && result.Captured.Value)
            {
                error = null;
                return true;
            }
            else
            {
                error = result.FailureMessage;
                return false;
            }
        }
    }
}
