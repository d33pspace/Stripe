using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stripe;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Data
{
    public class SubscriptionsFacade
    {
        private readonly ISubscriptionDataService _subscriptionDataService;
        private readonly ISubscriptionPlanDataService _subscriptionPlanDataService;
        private readonly ISubscriptionProvider _subscriptionProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly IChargeProvider _chargeProvider;
        private readonly ICardProvider _cardProvider;
        private readonly ICardDataService _cardDataService;

        public SubscriptionsFacade(
            ISubscriptionDataService data,
            ISubscriptionProvider subscriptionProvider,
            ICardProvider cardProvider,
            ICardDataService cardDataService,
            ICustomerProvider customerProvider,
            ISubscriptionPlanDataService subscriptionPlanDataService,
            IChargeProvider chargeProvider)
        {
            _subscriptionDataService = data;
            _subscriptionProvider = subscriptionProvider;
            _cardProvider = cardProvider;
            _customerProvider = customerProvider;
            _subscriptionPlanDataService = subscriptionPlanDataService;
            _chargeProvider = chargeProvider;
            _cardDataService = cardDataService;
        }

        public async Task<Subscription> SubscribeUserAsync
            (ApplicationUser user, string planId, decimal taxPercent = 0, CreditCard creditCard = null)
        {
            Subscription subscription;
            
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                subscription = await _subscriptionDataService.SubscribeUserAsync(user, planId, trialPeriodInDays: null, taxPercent: taxPercent);

                var cardToken = creditCard == null ? null : creditCard.StripeToken;
                var stripeUser = (StripeCustomer) await _customerProvider.CreateCustomerAsync(user, planId, null, cardToken);
                user.StripeCustomerId = stripeUser.Id;        

                subscription.StripeId = GetStripeSubscriptionIdForNewCustomer(stripeUser);
                await _subscriptionDataService.UpdateSubscriptionAsync(subscription);
            }
            else        
            {
                subscription = await this.SubscribeUserAsync(user, planId, creditCard, 0, taxPercent: taxPercent);
            }

            if (taxPercent > 0)
            {
                await this.UpdateSubscriptionTax(user, subscription.StripeId, taxPercent);
            }

            return subscription;
        }

        private async Task<Subscription> SubscribeUserAsync(ApplicationUser user, string planId, CreditCard creditCard, int trialInDays = 0, decimal taxPercent = 0)
        {
            if (creditCard != null)
            {
                if (creditCard.Id == 0)
                {
                    await _cardProvider.AddAsync(user, creditCard);
                }
                else
                {
                    await _cardProvider.UpdateAsync(user, creditCard);
                }
            }

            var subscriptionId = _subscriptionProvider.SubscribeUser
                (user, planId, trialInDays: trialInDays, taxPercent: taxPercent);  
            var subscription = await this._subscriptionDataService.SubscribeUserAsync(user, planId, trialInDays, taxPercent, subscriptionId);  

            return subscription;
        }


        public async Task<bool> UpdateSubscriptionTax(ApplicationUser user, string subscriptionId, decimal taxPercent)
        {
            await _subscriptionDataService.UpdateSubscriptionTax(subscriptionId, taxPercent);

            return _subscriptionProvider.UpdateSubscriptionTax(user.StripeCustomerId, subscriptionId, taxPercent);
        }

        public async Task<DateTime?> EndSubscriptionAsync(int subscriptionId, 
            ApplicationUser user, bool cancelAtPeriodEnd = false, string reasonToCancel = null)
        {
            DateTime? subscriptionEnd = null;
            try
            {
                var subscription = await _subscriptionDataService.UserActiveSubscriptionAsync(user.Id);
                if (subscription != null && subscription.Id == subscriptionId)
                {
                    subscriptionEnd = _subscriptionProvider.EndSubscription(user.StripeCustomerId, subscription.StripeId, cancelAtPeriodEnd);

                    await _subscriptionDataService.EndSubscriptionAsync(subscriptionId, subscriptionEnd.Value, reasonToCancel);
                }
            }
            catch (Exception)
            {
                subscriptionEnd = null;
            }

            return subscriptionEnd;
        }

        public async Task<bool> UpdateSubscriptionAsync(string userId, string stripeUserId, string newPlanId, bool proRate = true)
        {
            var activeSubscription = await _subscriptionDataService.UserActiveSubscriptionAsync(userId);

            if (activeSubscription != null &&
                (activeSubscription.SubscriptionPlan.Id != newPlanId || activeSubscription.End != null))          
            {
                if (_subscriptionProvider.UpdateSubscription(stripeUserId, activeSubscription.StripeId, newPlanId, proRate))
                {
                    activeSubscription.SubscriptionPlanId = newPlanId;
                    activeSubscription.End = null;       
                    await _subscriptionDataService.UpdateSubscriptionAsync(activeSubscription);
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> UpdateSubscriptionAsync(string userId, string stripeUserId, string stripeSubscriptionId, string newPlanId, bool proRate = true)
        {
            var subscription = _subscriptionDataService.FindById(stripeSubscriptionId);

            if (subscription != null &&
                (subscription.SubscriptionPlan.Id != newPlanId || subscription.End != null))          
            {
                bool changingPlan = subscription.SubscriptionPlan.Id != newPlanId;

                var currentPlan = await _subscriptionPlanDataService.FindAsync(subscription.SubscriptionPlanId);
                var newPlan = await _subscriptionPlanDataService.FindAsync(newPlanId);

                if (changingPlan && currentPlan.Price < newPlan.Price)
                {
                    var upgradeCharge = await CalculateProRata(newPlanId) - await CalculateProRata(subscription.SubscriptionPlanId);

                    var upgradeChargeWithTax = upgradeCharge*(1 + subscription.TaxPercent/100);

                    string error;
                    _chargeProvider.CreateCharge((int)upgradeChargeWithTax, await GetPlanCurrency(newPlanId), "Fluxifi Upgrade", stripeUserId, out error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        return false;
                    }
                }
                
                if (_subscriptionProvider.UpdateSubscription(stripeUserId, subscription.StripeId, newPlanId, proRate))
                {
                    subscription.SubscriptionPlanId = newPlanId;
                    subscription.End = null;       
                    await _subscriptionDataService.UpdateSubscriptionAsync(subscription);
                    return true;
                }
            }

            return false;
        }

        public async Task<CreditCard> DefaultCreditCard(string userId)
        {
            return (await _cardProvider.GetAllAsync(userId)).FirstOrDefault();
        }

        public async Task<List<Subscription>> UserActiveSubscriptionsAsync(string userId)
        {
            return await _subscriptionDataService.UserActiveSubscriptionsAsync(userId);
        }

        public async Task<int> DaysTrialLeftAsync(string userId)
        {
            var currentSubscription = (await this.UserActiveSubscriptionsAsync(userId)).FirstOrDefault();

            if (currentSubscription == null)
            {
                return 0;
            }
            else if (currentSubscription.IsTrialing())
            {
                var currentDate = DateTime.UtcNow;
                TimeSpan? timeSpan = currentSubscription.TrialEnd - currentDate;

                return timeSpan.Value.Hours > 12 ? timeSpan.Value.Days + 1 : timeSpan.Value.Days;
            }

            return 0;
        }

        public async Task SubscribeUserNaturalMonthAsync(ApplicationUser user, string planId, CreditCard card, decimal taxPercent = 0)
        {
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var stripeUser = (StripeCustomer)await _customerProvider.CreateCustomerAsync(user, cardToken: card.StripeToken);
                user.StripeCustomerId = stripeUser.Id;
                card.SaasEcomUserId = user.Id;
                await _cardDataService.AddAsync(card);
            }
            else if (!string.IsNullOrEmpty(card?.StripeToken))
            {
                var customer = (StripeCustomer)_customerProvider.UpdateCustomer(user, card);
                card.SaasEcomUserId = user.Id;
                card.StripeId = customer.DefaultSourceId;
                await _cardDataService.AddOrUpdateDefaultCardAsync(user.Id, card);
            }

            var stripeSubscription = (StripeSubscription)_subscriptionProvider.SubscribeUserNaturalMonth(user, planId, GetStartNextMonth(), taxPercent);
            await _subscriptionDataService.SubscribeUserAsync(user, planId, (int?)null, taxPercent, stripeSubscription.Id);
        }

        public async Task DeleteSubscriptions(string userId)
        {
            await this._subscriptionDataService.DeleteSubscriptionsAsync(userId);
        }

        #region Helpers
        private async Task<string> GetPlanCurrency(string planId)
        {
            var plan = await _subscriptionPlanDataService.FindAsync(planId);

            return plan.Currency;
        }

        private async Task<int> CalculateProRata(string planId)
        {
            var plan = await _subscriptionPlanDataService.FindAsync(planId);

            var now = DateTime.UtcNow;
            var beginningMonth = new DateTime(now.Year, now.Month, 1);
            var endMonth = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59);

            var totalHoursMonth = (endMonth - beginningMonth).TotalHours;
            var hoursRemaining = (endMonth - now).TotalHours;

            var amountInCurrency = plan.Price * hoursRemaining / totalHoursMonth;

            switch (plan.Currency.ToLower())
            {
                case ("usd"):
                case ("gbp"):
                case ("eur"):
                    return (int)Math.Ceiling(amountInCurrency * 100);
                default:
                    return (int)Math.Ceiling(amountInCurrency);
            }
        }

        private DateTime? GetStartNextMonth()
        {
            var now = DateTime.UtcNow;
            var year = now.Month == 12 ? now.Year + 1 : now.Year;
            var month = now.Month == 12 ? 1 : now.Month + 1;

            return new DateTime(year, month, 1);
        }

        private string GetStripeSubscriptionIdForNewCustomer(StripeCustomer stripeUser)
        {
            return stripeUser.StripeSubscriptionList.TotalCount > 0 ? 
                stripeUser.StripeSubscriptionList.Data.First().Id : null;
        }
        #endregion
    }

    public interface ICardProvider
    {
        Task AddAsync(ApplicationUser user, CreditCard card);

        Task UpdateAsync(ApplicationUser user, CreditCard creditcard);

        Task DeleteAsync(string customerId, string custStripeId, int cardId);

        Task<IList<CreditCard>> GetAllAsync(string customerId);

        Task<CreditCard> FindAsync(string userId, int? cardId);

        Task<bool> CardBelongToUser(int cardId, string userId);
    }

    public interface IChargeProvider
    {
        bool CreateCharge(int amount, string currency, string description, string customerId, out string error);

    }

    public interface ICustomerProvider
    {
        Task<object> CreateCustomerAsync(ApplicationUser user, string planId = null, DateTime? trialEnd = null, string cardToken = null);

        object UpdateCustomer(ApplicationUser user, CreditCard card);

        void DeleteCustomer(ApplicationUser user);
    }

    public interface ISubscriptionPlanProvider
    {
        object Add(SubscriptionPlan plan);

        object Update(SubscriptionPlan plan);

        void Delete(string planId);

        SubscriptionPlan FindAsync(string planId);

        IEnumerable<SubscriptionPlan> GetAllAsync(object options);
    }

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
