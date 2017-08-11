using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Services
{
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<StripeSettings> _stripeSettings;

        public DonationService(ApplicationDbContext dbContext, IOptions<StripeSettings> stripeSettings)
        {
            _dbContext = dbContext;
            _stripeSettings = stripeSettings;
        }

        public Dictionary<PaymentCycle, string> GetCycles()
        {
            return EnumInfo<PaymentCycle>
                .GetValues()
                .ToDictionary(o => o.Key, o => o.Value);
        }

        public void Save(Donation donation)
        {
            _dbContext.Donations.Add(donation);
            _dbContext.SaveChanges();
        }

        public Donation GetById(int id)
        {
            return _dbContext.Donations.Find(id);
        }

        public IEnumerable<string> GetPlans()
        {
            var options = new DonationViewModel().DonationOptions;
            foreach (var cycle in GetCycles())
                foreach(var option in options)
                    if (cycle.Key != PaymentCycle.OneOff)
                        if(option.Amount == 0)
                            yield return $"{cycle.Value}_custom";
                        else
                            yield return $"{cycle.Value}_{option.Amount}";
        }

        public StripePlan GetOrCreatePlan(Donation donation)
        {
            var planService = new StripePlanService(_stripeSettings.Value.SecretKey);

            var cycle = EnumInfo<PaymentCycle>.GetValue(donation.CycleId);
            var frequency = EnumInfo<PaymentCycle>.GetDescription(cycle);
            var amount = donation.DonationAmount != null ? donation.DonationAmount.Value : 0;
            if (donation.DonationAmount == null)
            {
                var model = (DonationViewModel) donation;
                amount = model.GetDisplayAmount();
            }
            var planName = $"{frequency}_{amount}".ToLower();

            if (!Exists(planService, planName))
            {
                var plan = new StripePlanCreateOptions
                {
                    Id = planName,
                    Amount = amount * 100,
                    Currency = "usd",
                    Interval = frequency, // day/month/year
                    Name = planName
                };
                return planService.Create(plan);
            }
            else
                return planService.Get(planName);
        }

        public int GetByUserId(string userId)
        {
            return _dbContext.Donations.Last(d => d.UserId == userId).Id;
        }

        public void EnsurePlansExist()
        {
            var planService = new StripePlanService(_stripeSettings.Value.SecretKey);

            var options = new DonationViewModel().DonationOptions;
            foreach (var cycle in GetCycles())
            {
                foreach (var option in options)
                {
                    if (cycle.Key != PaymentCycle.OneOff)
                    {
                        if (option.Amount > 0)
                        {
                            var planName = $"{cycle.Value}_{option.Amount}".ToLower();
                            var plan = new StripePlanCreateOptions
                            {
                                Id = planName,
                                Amount = option.Amount * 100,
                                Currency = "usd",
                                Interval = cycle.Key.ToString().ToLower(), // day/month/year
                                Name = planName
                            };
                            if (!Exists(planService, planName))
                                planService.Create(plan);
                        }
                    }
                }
            }
        }

        private bool Exists(StripePlanService planService, string planName)
        {
            try
            {
                planService.Get(planName);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}