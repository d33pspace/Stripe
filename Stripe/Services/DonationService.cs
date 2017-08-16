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

        public List<DonationListOption> DonationOptions => new List<DonationListOption>
        {
            new DonationListOption {Id = 1, Amount = 25, Reason = "to provide one day of showers, laundry and care for five people."},
            new DonationListOption {Id = 2, Amount = 50, Reason = "to provide a week of shelter and training for one person."},
            new DonationListOption {Id = 3, Amount = 100, Reason = "towards shower renovations or the purchase of a new van."},
            new DonationListOption {Id = 4, Amount = 0, Reason = "to help as many people as possible today!", IsCustom = true},
        };

        public void Save(Donation donation)
        {
            _dbContext.Donations.Add(donation);
            _dbContext.SaveChanges();
        }

        public Donation GetById(int id)
        {
            return _dbContext.Donations.Find(id);
        }

        /// <summary>
        /// Create plan for this donation if is does not exist and return its instance. If it does exist
        /// return the instance.
        /// </summary>
        /// <param name="donation"></param>
        /// <returns></returns>
        public StripePlan GetOrCreatePlan(Donation donation)
        {
            var planService = new StripePlanService(_stripeSettings.Value.SecretKey);

            // Construct plan name from the selected donation type and the cycle
            var cycle = EnumInfo<PaymentCycle>.GetValue(donation.CycleId);
            var frequency = EnumInfo<PaymentCycle>.GetDescription(cycle);
            var amount = donation.DonationAmount ?? 0;
            if (donation.DonationAmount == null)
            {
                var model = (DonationViewModel) donation;
                model.DonationOptions = DonationOptions;

                amount = model.GetDisplayAmount();
            }
            var planName = $"{frequency}_{amount}".ToLower();

            // Create new plan is this one does not exist
            if (!Exists(planService, planName))
            {
                var plan = new StripePlanCreateOptions
                {
                    Id = planName,
                    Amount = amount * 100,
                    Currency = "usd",
                    Name = planName, 
                    StatementDescriptor = _stripeSettings.Value.StatementDescriptor
                };
                
                // Take care intervals
                if (cycle == PaymentCycle.Quarter)
                {
                    plan.IntervalCount = 3;
                    plan.Interval = "month";
                }
                else
                {
                    plan.Interval = cycle.ToString().ToLower(); // day/month/year 
                }
                return planService.Create(plan);
            }
            else
                return planService.Get(planName);
        }

        public int GetByUserId(string userId)
        {
            return _dbContext.Donations.Last(d => d.UserId == userId).Id;
        }

        /// <summary>
        /// Automatically create the standard plans to enable, new users to be able to subscribe. These
        /// are managed in Stripe
        /// </summary>
        public void EnsurePlansExist()
        {
            var planService = new StripePlanService(_stripeSettings.Value.SecretKey);

            var options = new DonationViewModel(DonationOptions).DonationOptions;
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
                                Name = planName,
                                StatementDescriptor = _stripeSettings.Value.StatementDescriptor
                            };

                            // Take care intervals
                            if (cycle.Key == PaymentCycle.Quarter)
                            {
                                plan.IntervalCount = 3;
                                plan.Interval = "month";
                            }
                            else
                            {
                                plan.Interval = cycle.Key.ToString().ToLower(); // day/month/year 
                            }

                            if (!Exists(planService, planName))
                                planService.Create(plan);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check is the plan exists. The API does not have an exists endpoint so we have to use an
        /// exception to detemine existence. 
        /// </summary>
        /// <param name="planService">The StripePlanService Instance</param>
        /// <param name="planName">The Plan name</param>
        /// <returns></returns>
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