using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe.Data;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Services
{
    public class SubscriptionPlanDataService<TContext, TUser> : ISubscriptionPlanDataService
        where TContext : ApplicationDbContext
        where TUser : class
    {
        private readonly TContext _dbContext;

        public SubscriptionPlanDataService(TContext context)
        {
            this._dbContext = context;
        }

        public Task<List<SubscriptionPlan>> GetAllAsync()
        {
            return _dbContext.SubscriptionPlans.ToListAsync();
        }


        public Task<SubscriptionPlan> FindAsync(string planId)
        {
            return _dbContext.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == planId);
        }

        public Task AddAsync(SubscriptionPlan plan)
        {
            _dbContext.SubscriptionPlans.Add(plan);
            return _dbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(SubscriptionPlan plan)
        {
            var dbPlan = await FindAsync(plan.Id);
            dbPlan.Name = plan.Name;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAsync(string planId)
        {
            var dbPlan = await FindAsync(planId);
            _dbContext.SubscriptionPlans.Remove(dbPlan);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> DisableAsync(string id)
        {
            var dbPlan = await FindAsync(id);
            dbPlan.Disabled = true;
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountUsersAsync(string planId)
        {
            var count = await _dbContext.Subscriptions
                .Where(s => s.End == null || s.End > DateTime.UtcNow)
                .CountAsync(s => s.SubscriptionPlanId == planId);
            return count;
        }
    }
}
