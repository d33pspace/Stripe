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
        
        private static string GetInterval(PaymentCycle interval)
        {
            string result = null;

            switch (interval)
            {
                case PaymentCycle.Monthly:
                    result = "month";
                    break;
                case PaymentCycle.Annualy:
                    result = "year";
                    break;
                case PaymentCycle.Quarterly:
                    result = "quarter";
                    break;
                case PaymentCycle.OneOff:
                    result = "oneoff";
                    break;
            }

            return result;
        }

        private static PaymentCycle GetInterval(string interval)
        {
            switch (interval)
            {
                case "month":
                    return PaymentCycle.Monthly;
                case "year":
                    return PaymentCycle.Annualy;
                case "quarter":
                    return PaymentCycle.Quarterly;
                case "oneoff":
                    return PaymentCycle.OneOff;
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


    public class BillingCycle : IBillingCycle
    {
        public List<KeyValuePair<PaymentCycle, string>> GetCycles()
        {
            return EnumInfo<PaymentCycle>.GetValues();
        }
    }

    public interface IBillingCycle
    {
        List<KeyValuePair<PaymentCycle, string>> GetCycles();
    }
}
