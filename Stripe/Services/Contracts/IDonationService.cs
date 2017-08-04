using System.Collections.Generic;
using Stripe.Models;

namespace Stripe.Services
{
    public interface IDonationService
    {
        List<KeyValuePair<PaymentCycle, string>> GetCycles();
        void Save(Donation donation);
        Donation GetById(int id);
    }
}