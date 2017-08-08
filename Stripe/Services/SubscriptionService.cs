using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe.Data;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<StripeSettings> _stripeOptions;

        public SubscriptionService(ApplicationDbContext context, IOptions<StripeSettings> stripeOptions)
        {
            this._dbContext = context;
            _stripeOptions = stripeOptions;
        }

        public Subscription FindById(string stripeSubscriptionId)
        {
            return _dbContext.Subscriptions.FirstOrDefault(s => s.StripeId == stripeSubscriptionId);
        }

        public async Task<Subscription> SubscribeUserAsync(ApplicationUser user, string planId, int? trialPeriodInDays = null, decimal taxPercent = 0, string stripeId = null)
        {
//            var plan = await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.Id == planId);
//
//            if (plan == null)
//            {
//                throw new ArgumentException(string.Format("There's no plan with Id: {0}", planId));
//            }

            var s = new Subscription
            {
                UserId = user.Id,
                Status = trialPeriodInDays == null ? "active" : "trialing",
                StripeId = stripeId
            };

            _dbContext.Subscriptions.Add(s);
            await _dbContext.SaveChangesAsync();

            return s;
        }

        public async Task<Subscription> UserActiveSubscriptionAsync(string userId)
        {
            return (await UserActiveSubscriptionsAsync(userId)).FirstOrDefault();
        }

        public async Task<List<Subscription>> UserSubscriptionsAsync(string userId)
        {
            return await _dbContext.Subscriptions.Where(s => s.User.Id == userId).Select(s => s).ToListAsync();
        }

        public async Task<List<Subscription>> UserActiveSubscriptionsAsync(string userId)
        {
            return await _dbContext.Subscriptions
                .Where(s => s.User.Id == userId && s.Status != "canceled" && s.Status != "unpaid")
                .Select(s => s).ToListAsync();
        }

        public async Task EndSubscriptionAsync(int subscriptionId, DateTime subscriptionEnDateTime, string reasonToCancel)
        {
            var dbSub = await _dbContext.Subscriptions.FindAsync(subscriptionId);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSubscriptionAsync(Subscription subscription)
        {
            _dbContext.Entry(subscription).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSubscriptionsAsync(string userId)
        {
            foreach (var subscription in _dbContext.Subscriptions.Where(s => s.UserId == userId).Select(s =>s))
            {
                _dbContext.Subscriptions.Remove(subscription);
            }

            await _dbContext.SaveChangesAsync();
        }

        public void CreateSubscription(Subscription subscription)
        {
            var plan = new StripePlanCreateOptions
            {
                Id = subscription.SubscriptionPlanId,
                Amount = subscription.Amount,
                Currency = subscription.Currency,
                Interval = subscription.Interval,
                Name = subscription.Name
            };
            // "usd" only supported right now
            // "month" or "year"

            var planService = new StripePlanService(_stripeOptions.Value.SecretKey);
            var response = planService.Create(plan);

            _dbContext.Subscriptions.Add(subscription);
            _dbContext.SaveChanges();
        }
    }
}
