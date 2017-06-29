using System;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Data
{
    public interface ICustomerProvider
    {
        Task<object> CreateCustomerAsync(ApplicationUser user, string planId = null, DateTime? trialEnd = null, string cardToken = null);

        object UpdateCustomer(ApplicationUser user, CreditCard card);

        void DeleteCustomer(ApplicationUser user);
    }
}