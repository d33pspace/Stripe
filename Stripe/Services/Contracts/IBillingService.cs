using System.Collections.Generic;
using Stripe.Models;

namespace Stripe.Services
{
    public interface IBillingService
    {
        List<KeyValuePair<PaymentCycle, string>> GetCycles();
        void SaveUserBill(Donation donation);
        Donation GetById(int id);
    }
}