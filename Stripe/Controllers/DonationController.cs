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

        public IActionResult CreditCard(int id)
        {
            var donation = _donationService.GetById(id);
            var model = new CustomerPaymentViewModel
            {
                DonationId = donation.Id,
                CycleId = donation.CycleId
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreditCard(CustomerPaymentViewModel payment)
        {
            if (!ModelState.IsValid)
                return View();

            var user = await GetCurrentUserAsync();
            var donation = _donationService.GetById(payment.DonationId);
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                var model = (DonationViewModel)donation;

                return View("Thanks");
            }
            else
            {
                // Add to subscription
                var plan = _donationService.GetOrCreatePlan(donation);
                var customer = new StripeCustomerCreateOptions();
                customer.Email = user.Email;
                customer.Description = $"{user.Email} {user.Id}";
                customer.SourceCard = new SourceCard
                {
                    Number = payment.CardNumber,
                    Cvc = payment.Cvc,
                    ExpirationMonth = payment.ExpiryMonth,
                    ExpirationYear = payment.ExpiryYear
                };

                var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);
                var stripeCustomer = customerService.Create(customer);

                var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
                subscriptionService.Create(stripeCustomer.Id, plan.Id);

                // update user
                user.StripeCustomerId = stripeCustomer.Id;
                await _userManager.UpdateAsync(user);
            }
            return View("Thanks");
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Charge(string stripeEmail, string stripeToken, string description, int donationAmount)
        {
            var customers = new StripeCustomerService(_stripeSettings.Value.SecretKey);
            var charges = new StripeChargeService(_stripeSettings.Value.SecretKey);

            var customer = customers.Create(new StripeCustomerCreateOptions
            {
                Email = stripeEmail,
                SourceToken = stripeToken
            });

            var charge = charges.Create(new StripeChargeCreateOptions
            {
                Amount = donationAmount,
                Description = description,
                Currency = "usd",
                CustomerId = customer.Id
            });
            return View(charge);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }


    }

    public class SubscriptionViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }

    public class CustomerPaymentViewModel
    {
        public string UserName { get; set; }
        public List<SubscriptionViewModel> Subscriptions { get; set; }
        public string CardNumber { get; set; }
        public string Cvc { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public int DonationId { get; set; }
        public string CycleId { get; set; }
    }
}