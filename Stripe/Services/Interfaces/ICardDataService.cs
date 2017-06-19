﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Services
{
    public interface ICardDataService
    {
        Task<IList<CreditCard>> GetAllAsync(string userId);

        Task<CreditCard> FindAsync(string userId, int? cardId, bool noTracking = false);

        Task AddAsync(CreditCard creditcard);

        Task AddOrUpdateDefaultCardAsync(string userId, CreditCard creditCard);

        Task UpdateAsync(string userId, CreditCard creditCard);

        Task DeleteAsync(string userId, int cardId);

        Task<bool> AnyAsync(int? cardId, string userId);
    }
}
