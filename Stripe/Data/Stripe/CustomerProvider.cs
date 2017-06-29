using System;
using System.Threading.Tasks;
using Stripe;
using Stripe.Models;

namespace Stripe.Data
{
    public class CustomerProvider : ICustomerProvider
    {
        private readonly StripeCustomerService _customerService;

        public CustomerProvider()
        {
            _customerService = new StripeCustomerService("pk_test_W9r8jIofygDYmsy7gUcjrVEG");
        }

        public async Task<object> CreateCustomerAsync(ApplicationUser user, string planId = null, DateTime? trialEnd = null, string cardToken = null)
        {
            var customer = new StripeCustomerCreateOptions
            {
                AccountBalance = 0,
                Email = user.Email
            };

            if (!string.IsNullOrEmpty(cardToken))
            {
                customer.SourceToken = cardToken;
            }

            if (!string.IsNullOrEmpty(planId))
            { 
                customer.PlanId = planId;
                customer.TrialEnd = trialEnd;
            }

            var stripeUser = await Task.Run(() => _customerService.Create(customer));
            return stripeUser;
        }

        public object UpdateCustomer(ApplicationUser user, CreditCard card)
        {
            var customer = new StripeCustomerUpdateOptions
            {
                Email = user.Email,

                SourceToken = card.StripeToken
            };

            return _customerService.Update(user.StripeCustomerId, customer);
        }

        public void DeleteCustomer(ApplicationUser user)
        {
            _customerService.Delete(user.StripeCustomerId);
        }
    }
}
