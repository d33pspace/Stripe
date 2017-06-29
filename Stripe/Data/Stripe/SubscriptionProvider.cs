using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using Stripe.Models;

namespace Stripe.Data
{
    public class SubscriptionProvider : ISubscriptionProvider
    {
        private readonly StripeSubscriptionService _subscriptionService;

        public SubscriptionProvider()
        {
            this._subscriptionService = new StripeSubscriptionService("pk_test_W9r8jIofygDYmsy7gUcjrVEG");
        }

        public string SubscribeUser(ApplicationUser user, string planId, int trialInDays = 0, decimal taxPercent = 0)
        {
            var result = this._subscriptionService.Update(user.StripeCustomerId, planId,
                new StripeSubscriptionUpdateOptions
                {
                    PlanId = planId,
                    TaxPercent = taxPercent,
                    TrialEnd = DateTime.UtcNow.AddDays(trialInDays)
                });

            return result.Id;
        }

        public string SubscribeUser(ApplicationUser user, string planId, DateTime? trialEnds, decimal taxPercent = 0)
        {
            var result = this._subscriptionService.Update(user.StripeCustomerId, planId,
                new StripeSubscriptionUpdateOptions
                {
                    PlanId = planId,
                    TaxPercent = taxPercent,
                    TrialEnd = trialEnds
                });

            return result.Id;
        }

        public Task<List<Subscription>> UserSubscriptionsAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public DateTime EndSubscription(string userStripeId, string subStripeId, bool cancelAtPeriodEnd = false)
        {
            var subscription = this._subscriptionService.Cancel(userStripeId, subStripeId, cancelAtPeriodEnd);

            return cancelAtPeriodEnd ? subscription.EndedAt.Value : DateTime.UtcNow;
        }

        public bool UpdateSubscription(string customerId, string subStripeId, string newPlanId, bool proRate)
        {
            var result = true;
            try
            {
                var currentSubscription = this._subscriptionService.Get(customerId, subStripeId);

                var myUpdatedSubscription = new StripeSubscriptionUpdateOptions
                {
                    PlanId = newPlanId,
                    Prorate = proRate
                };

                if (currentSubscription.TrialEnd != null && currentSubscription.TrialEnd > DateTime.UtcNow)
                {
                    myUpdatedSubscription.TrialEnd = currentSubscription.TrialEnd;         
                }

                _subscriptionService.Update(customerId, subStripeId, myUpdatedSubscription);
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public bool UpdateSubscriptionTax(string customerId, string subStripeId, decimal taxPercent = 0)
        {
            var result = true;
            try
            {
                var myUpdatedSubscription = new StripeSubscriptionUpdateOptions
                {
                    TaxPercent = taxPercent
                };
                _subscriptionService.Update(customerId, subStripeId, myUpdatedSubscription);
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        public object SubscribeUserNaturalMonth(ApplicationUser user, string planId, DateTime? billingAnchorCycle, decimal taxPercent)
        {
            StripeSubscription stripeSubscription = _subscriptionService.Create
                (user.StripeCustomerId, planId, new StripeSubscriptionCreateOptions
                {
                    BillingCycleAnchor = billingAnchorCycle,
                    TaxPercent = taxPercent
                });

            return stripeSubscription;
        }
    }
}
