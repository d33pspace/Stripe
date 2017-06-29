using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Data
{
    public interface ICardProvider
    {
        Task AddAsync(ApplicationUser user, CreditCard card);

        Task UpdateAsync(ApplicationUser user, CreditCard creditcard);

        Task DeleteAsync(string customerId, string custStripeId, int cardId);

        Task<IList<CreditCard>> GetAllAsync(string customerId);

        Task<CreditCard> FindAsync(string userId, int? cardId);

        Task<bool> CardBelongToUser(int cardId, string userId);
    }
}