using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Data
{
    public class CardProvider : ICardProvider
    {
        private readonly ICardDataService _cardDataService;
        private readonly StripeCardService _cardService;

        public CardProvider(ICardDataService cardDataService)
        {
            this._cardDataService = cardDataService;
            this._cardService = new StripeCardService("pk_test_W9r8jIofygDYmsy7gUcjrVEG");
        }

        public async Task<IList<CreditCard>> GetAllAsync(string customerId)
        {
            return await _cardDataService.GetAllAsync(customerId);
        }

        public async Task<CreditCard> FindAsync(string customerId, int? cardId)
        {
            return await _cardDataService.FindAsync(customerId, cardId);
        }

        public async Task AddAsync(ApplicationUser user, CreditCard card)
        {
            var stripeCustomerId = user.StripeCustomerId;
            AddCardToStripe(card, stripeCustomerId);

            card.UserId = user.Id;
            await _cardDataService.AddAsync(card);
        }

        public async Task UpdateAsync(ApplicationUser user, CreditCard creditcard)
        {
            var currentCard = await _cardDataService.FindAsync(user.Id, creditcard.Id, true);
            var stripeCustomerId = user.StripeCustomerId;
            _cardService.Delete(stripeCustomerId, currentCard.StripeId);

            this.AddCardToStripe(creditcard, stripeCustomerId);

            creditcard.UserId = user.Id;
            await _cardDataService.UpdateAsync(user.Id, creditcard);
        }

        public async Task<bool> CardBelongToUser(int cardId, string userId)
        {
            return await this._cardDataService.AnyAsync(cardId, userId);
        }

        public async Task DeleteAsync(string customerId, string custStripeId, int cardId)
        {
            var card = await this._cardDataService.FindAsync(customerId, cardId, true);

            this._cardService.Delete(custStripeId, card.StripeId);
            await this._cardDataService.DeleteAsync(customerId, cardId);
        }

        private StripeCard AddCardToStripe(CreditCard card, string stripeCustomerId)
        {
            var options = new StripeCardCreateOptions
            {
                SourceCard = new SourceCard
                {
                    Number = card.CardNumber,
                    ExpirationMonth = int.Parse(card.ExpirationMonth),
                    ExpirationYear = int.Parse(card.ExpirationYear),
                    AddressCountry = card.AddressCountry,
                    AddressLine1 = card.AddressLine1,
                    AddressCity = card.AddressCity,
                    AddressState = card.AddressState,
                    AddressZip = card.AddressZip,
                    Name = card.Name,
                    Cvc = card.Cvc,
                }
            };

            return _cardService.Create(stripeCustomerId, options);
        }
    }
}
