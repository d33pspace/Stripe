using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Services
{
    public class CardDataService : ICardDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public CardDataService(ApplicationDbContext context)
        {
            this._dbContext = context;
        }

        public async Task<IList<CreditCard>> GetAllAsync(string userId)
        {
            var user = await this._dbContext.Users.Include(u => u.CreditCards)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("Customer Id: {0} doesn't exist.", userId);
            }

            return user.CreditCards;
        }

        public async Task<CreditCard> FindAsync(string userId, int? cardId, bool noTracking = false)
        {
            if (noTracking)
            {
                return await this._dbContext.CreditCards.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cardId);
            }

            return await this._dbContext.CreditCards
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cardId);
        }

        public async Task<bool> AnyAsync(int? cardId, string userId)
        {
            return await this._dbContext.CreditCards
                .AnyAsync(c => c.UserId == userId && c.Id == cardId);
        }

        public async Task AddAsync(CreditCard creditCard)
        {
            _dbContext.CreditCards.Add(creditCard);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateDefaultCardAsync(string userId, CreditCard creditCard)
        {
            var card = this._dbContext.CreditCards.FirstOrDefault(c => c.UserId == userId);

            if (card != null)
            {
                _dbContext.CreditCards.Remove(card);
                await _dbContext.SaveChangesAsync();
            }

            await this.AddAsync(creditCard);
        }

        public async Task UpdateAsync(string userId, CreditCard creditCard)
        {
            if (!this._dbContext.CreditCards.Any(c => c.UserId == userId && c.Id == creditCard.Id))
            {
                throw new ArgumentException("cardId");
            }

            _dbContext.Entry(creditCard).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(string userId, int cardId)
        {
            CreditCard creditcard = await this._dbContext.CreditCards
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == cardId);

            if (creditcard == null)
            {
                throw new ArgumentException("cardId");
            }

            _dbContext.CreditCards.Remove(creditcard);
            await _dbContext.SaveChangesAsync();
        }
    }
}
