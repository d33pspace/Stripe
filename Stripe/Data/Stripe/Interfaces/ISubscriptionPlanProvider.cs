using System.Collections.Generic;
using Stripe.Models;

namespace Stripe.Data
{
    public interface ISubscriptionPlanProvider
    {
        object Add(SubscriptionPlan plan);

        object Update(SubscriptionPlan plan);

        void Delete(string planId);

        SubscriptionPlan FindAsync(string planId);

        IEnumerable<SubscriptionPlan> GetAllAsync(object options);
    }
}