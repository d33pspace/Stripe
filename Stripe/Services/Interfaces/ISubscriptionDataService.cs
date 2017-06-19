using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Services
{
    public interface ISubscriptionDataService
    {
        Subscription FindById(string stripeSubscriptionId);

        Task<Subscription> SubscribeUserAsync(ApplicationUser user, string planId, int? trialPeriodInDays = null,
            decimal taxPercent = 0, string stripeId = null);

        Task<Subscription> SubscribeUserAsync(ApplicationUser user, string planId, DateTime? trialPeriodEnds = null,
            decimal taxPercent = 0, string stripeId = null);

        Task<List<Subscription>> UserSubscriptionsAsync(string userId);

        Task<Subscription> UserActiveSubscriptionAsync(string userId);

        Task<List<Subscription>> UserActiveSubscriptionsAsync(string userId);

        Task EndSubscriptionAsync(int subscriptionId, DateTime subscriptionEnDateTime, string reasonToCancel);

        Task UpdateSubscriptionAsync(Subscription subscription);

        Task UpdateSubscriptionTax(string subscriptionId, decimal taxPercent);

        Task DeleteSubscriptionsAsync(string userId);
    }
}
