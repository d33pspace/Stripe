using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Services
{
    public interface ISubscriptionPlanDataService
    {
        Task<List<SubscriptionPlan>> GetAllAsync();

        Task<SubscriptionPlan> FindAsync(string subscriptionPlanId);

        Task AddAsync(SubscriptionPlan subscriptionPlan);

        Task<int> UpdateAsync(SubscriptionPlan subscriptionPlan);

        Task<int> DeleteAsync(string subscriptionPlanId);

        Task<int> DisableAsync(string subscriptionPlanId);

        Task<int> CountUsersAsync(string subscriptionPlanId);
    }
}
