using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Services
{
    public class CardService 
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<StripeSettings> _billingSettings;

        private string ApiKey { get; }

        public CardService(ApplicationDbContext context, IOptions<StripeSettings> billingSettings)
        {
            this._dbContext = context;
            _billingSettings = billingSettings;
            ApiKey = _billingSettings.Value.PublishableKey;
        }


        public async Task<string> GetTokenId(CreditCardViewModel model)
        {
            return await Task.Run(() =>
            {
                var createOptions = new StripeTokenCreateOptions
                {
                    Card = new StripeCreditCardOptions
                    {
                        AddressCountry = model.AddressCountry,
                        AddressLine1 = model.AddressLine1,
                        AddressLine2 = model.AddressLine2,
                        AddressCity = model.AddressCity,
                        AddressZip = model.AddressZip,
                        Cvc = model.Cvc,
                        ExpirationMonth = model.ExpirationMonth,
                        ExpirationYear = model.ExpirationYear,
                        Name = model.Name,
                        Number = model.CardNumber
                    }
                };

                var tokenService = new StripeTokenService(ApiKey);
                var stripeToken = tokenService.Create(createOptions);
                return stripeToken.Id;
            });
        }

 
    }
}
