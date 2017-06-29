using System;
using System.Collections.Generic;
using System.Linq;
using Stripe;
using Stripe.Models;

namespace Stripe.Data
{
    public class SubscriptionPlanProvider : ISubscriptionPlanProvider
    {
        private readonly string _apiKey;

        private StripePlanService _planService;
        private StripePlanService PlanService
        {
            get { return _planService ?? (_planService = new StripePlanService(_apiKey)); }
        }

        public SubscriptionPlanProvider()
        {
            _apiKey = "pk_test_W9r8jIofygDYmsy7gUcjrVEG";
        }

        public object Add(SubscriptionPlan plan)
        {
            var result = PlanService.Create(new StripePlanCreateOptions
            {
                Id = plan.Id,
                Name = plan.Name,
                Amount = (int)Math.Round(plan.Price * 100),
                Currency = plan.Currency,
                Interval = GetInterval(plan.Interval),
                TrialPeriodDays = plan.TrialPeriodInDays,
                IntervalCount = 1,                       
            });

            return result;
        }

        public object Update(SubscriptionPlan plan)
        {
            var res = PlanService.Update(plan.Id, new StripePlanUpdateOptions
            {
                Name = plan.Name
            });

            return res;
        }

        public void Delete(string planId)
        {
            PlanService.Delete(planId);
        }

        public SubscriptionPlan FindAsync(string planId)
        {
            try
            {
                var stripePlan = PlanService.Get(planId);

                return SubscriptionPlanMapper(stripePlan);
            }
            catch (StripeException ex)
            {
                return null;
            }
        }

        public IEnumerable<SubscriptionPlan> GetAllAsync(object options)
        {
            var result = PlanService.List((StripeListOptions)options);

            return result.Select(SubscriptionPlanMapper);
        }
        
        private static string GetInterval(SubscriptionPlan.SubscriptionInterval interval)
        {
            string result = null;

            switch (interval)
            {
                case (SubscriptionPlan.SubscriptionInterval.Monthly):
                    result = "month";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.Yearly):
                    result = "year";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.Weekly):
                    result = "week";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.EveryThreeMonths):
                    result = "3-month";
                    break;
                case (SubscriptionPlan.SubscriptionInterval.EverySixMonths):
                    result = "6-month";
                    break;
            }

            return result;
        }

        private static SubscriptionPlan.SubscriptionInterval GetInterval(string interval)
        {
            switch (interval)
            {
                case ("month"):
                    return SubscriptionPlan.SubscriptionInterval.Monthly;
                case ("year"):
                    return SubscriptionPlan.SubscriptionInterval.Yearly;
                case ("week"):
                    return SubscriptionPlan.SubscriptionInterval.Weekly;
                case ("3-month"):
                    return SubscriptionPlan.SubscriptionInterval.EveryThreeMonths;
                case ("6-month"):
                    return SubscriptionPlan.SubscriptionInterval.EverySixMonths;
            }

            return 0;
        }

        private static SubscriptionPlan SubscriptionPlanMapper(StripePlan stripePlan)
        {
            return new SubscriptionPlan
            {
                Id = stripePlan.Id,
                Name = stripePlan.Name,
                Currency = stripePlan.Currency,
                Interval = GetInterval(stripePlan.Interval),
                Price = stripePlan.Amount,
                TrialPeriodInDays = stripePlan.TrialPeriodDays ?? 0,

            };
        }
    }
}
