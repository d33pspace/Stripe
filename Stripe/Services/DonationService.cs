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

        public StripePlan GetPlan(Donation donation)
        {
            var planService = new StripePlanService(_stripeSettings.Value.SecretKey);
            //var item = (DonationViewModel) donation;
            var frequency = EnumInfo<PaymentCycle>.GetValue(donation.CycleId).ToString().ToLower();
            if (donation.DonationAmount == 0)
                return planService.List().FirstOrDefault(s => s.Id == $"{frequency}_custom");
            return planService.List().FirstOrDefault(s => s.Id == $"{frequency}_{donation.DonationAmount}");
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
                        if (option.Amount == 0)
                        {
                            var planName = $"{cycle.Value}_custom";
                            var plan = new StripePlanCreateOptions
                            {
                                Id = planName,
                                Amount = option.Amount,
                                Currency = "usd",
                                Interval = cycle.Key.ToString().ToLower(),
                                Name = planName
                            };
                            if (planService.Get(planName) == null)
                                planService.Create(plan);
                        }
                        else
                        {
                            var planName = $"{cycle.Value}_{option.Amount}";
                            var plan = new StripePlanCreateOptions
                            {
                                Id = planName,
                                Amount = option.Amount,
                                Currency = "usd",
                                Interval = cycle.Key.ToString().ToLower(),
                                Name = planName
                            };
                            if (planService.Get(planName) == null)
                                planService.Create(plan);
                        }
                    }
                }
            }
        }

    }
}