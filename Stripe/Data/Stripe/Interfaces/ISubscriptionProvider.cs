using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Data
{
    public interface ISubscriptionProvider
    {
        string SubscribeUser(ApplicationUser user, string planId, int trialInDays = 0, decimal taxPercent = 0);

        string SubscribeUser(ApplicationUser user, string planId, DateTime? trialEnds, decimal taxPercent = 0);

        Task<List<Subscription>> UserSubscriptionsAsync(string userId);

        DateTime EndSubscription(string userStripeId, string subStripeId, bool cancelAtPeriodEnd = false);

        bool UpdateSubscription(string customerId, string subStripeId, string newPlanId, bool proRate);

        bool UpdateSubscriptionTax(string customerId, string subStripeId, decimal taxPercent = 0);

        object SubscribeUserNaturalMonth(ApplicationUser user, string planId, DateTime? billingAnchorCycle, decimal taxPercent);
    }
}