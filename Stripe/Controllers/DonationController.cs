using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    [Authorize]
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationController(
            UserManager<ApplicationUser> userManager, 
            IDonationService donationService,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _donationService = donationService;
            _stripeSettings = stripeSettings;
        }

        public IActionResult Payment(int id)
        {
            var donation = _donationService.GetById(id);
            var model = new CustomerPaymentViewModel
            {
                DonationId = donation.Id,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(CustomerPaymentViewModel payment)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);
            var user = await GetCurrentUserAsync();
            var donation = _donationService.GetById(payment.DonationId);

            // Construct payment
            var customer = new StripeCustomerCreateOptions
            {
                Email = user.Email,
                Description = $"{user.Email} {user.Id}",
                SourceCard = new SourceCard
                {
                    Name = payment.Name,
                    Number = payment.CardNumber,
                    Cvc = payment.Cvc,
                    ExpirationMonth = payment.ExpiryMonth,
                    ExpirationYear = payment.ExpiryYear
                }
            };

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var stripeCustomer = customerService.Create(customer);
                user.StripeCustomerId = stripeCustomer.Id;
                await _userManager.UpdateAsync(user);
            }

            // Add customer to Stripe
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                var model = (DonationViewModel)donation;
                var charges = new StripeChargeService(_stripeSettings.Value.SecretKey);

                // Charge the customer
                var charge = charges.Create(new StripeChargeCreateOptions
                {
                    Amount = model.GetAmount(),
                    Description = model.GetFullDescription(),
                    Currency = "usd",
                    CustomerId = user.StripeCustomerId
                });

                if (charge.Paid)
                {
                    var completedMessage = new CompletedViewModel
                    {
                        Message = $"Thank you donating {model.GetDisplayAmount()} for the payment {model.GetDescription()} "
                    };
                    return View("Thanks", completedMessage);
                }
                return View("Error");
            }
            else
            {
                // Add to existing subscriptions and charge 
                var plan = _donationService.GetOrCreatePlan(donation);

                var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
                var result = subscriptionService.Create(user.StripeCustomerId, plan.Id);
                if (result != null)
                {
                    var completedMessage = new CompletedViewModel
                    {
                        Message = $"You have added a subscription {result.StripePlan.Name} for this donation"
                    };
                    return View("Thanks", completedMessage);
                }
                return View("Error");
            }
        }


        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }

}