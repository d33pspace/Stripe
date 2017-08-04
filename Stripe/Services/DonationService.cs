using System.Collections.Generic;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Services
{
    public class DonationService : IDonationService
    {
        private readonly ApplicationDbContext _dbContext;

        public DonationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<KeyValuePair<PaymentCycle, string>> GetCycles()
        {
            return EnumInfo<PaymentCycle>.GetValues();
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

        public List<Subscription> GetSubScriptions(string userId)
        {
            return new List<Subscription>();
        }

        public List<CreditCard> GetPaymentDetails(string userId)
        {
            return new List<CreditCard>();
        }
    }
}