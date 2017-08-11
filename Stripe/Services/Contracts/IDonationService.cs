using System.Collections.Generic;
using Stripe.Models;

namespace Stripe.Services
{
    public interface IDonationService
    {
        Dictionary<PaymentCycle, string> GetCycles();
        void Save(Donation donation);
        Donation GetById(int id);
        void EnsurePlansExist();
        StripePlan GetOrCreatePlan(Donation donation);
        int GetByUserId(string userId);
    }
}