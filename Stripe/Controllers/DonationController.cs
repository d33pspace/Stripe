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

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var id = _donationService.GetByUserId(user.Id);

            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var customerService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
                var subscriptions = customerService.List(user.StripeCustomerId);

                var customerSubscription = new CustomerSubscriptionViewModel
                {
                    DonationId = id,
                    UserName = user.Email,
                    Subscriptions = subscriptions.Select(s => new SubscriptionViewModel
                    {
                        Id = s.Id,
                        Name = s.StripePlan.Name,
                        Amount = s.StripePlan.Amount,
                        Currency = s.StripePlan.Currency,
                        Status = s.Status
                    }).ToList()
                };
                return View("Index", customerSubscription);
            }
            var subscription = new CustomerSubscriptionViewModel
            {
                DonationId = id,
                UserName = user.Email,
                Subscriptions = new List<SubscriptionViewModel>()
            };
            return View("Index", subscription);
        }

        [AllowAnonymous]
        public IActionResult Payment(int id)
        {
            var donation = _donationService.GetById(id);
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                var model = (DonationViewModel)donation;
                return View("Payment", model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Payment(CustomerSubscriptionViewModel model)
        {
            var user = await GetCurrentUserAsync();
            var donation = _donationService.GetById(model.DonationId);
            var plan = _donationService.GetOrCreatePlan(donation);

            var customer = new StripeCustomerCreateOptions();
            customer.Email = user.Email;
            customer.Description = $"{user.Email} {user.Id}";
            customer.SourceCard = new SourceCard
            {
                Number = model.CardNumber,
                Cvc = model.Cvc,
                ExpirationMonth = model.ExpiryMonth,
                ExpirationYear = model.ExpiryYear
            };

            var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);
            var stripeCustomer = customerService.Create(customer);

            var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
            subscriptionService.Create(stripeCustomer.Id, plan.Id);

            // update user
            user.StripeCustomerId = stripeCustomer.Id;
            await _userManager.UpdateAsync(user);

            var list = stripeCustomer.Subscriptions;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(string subscriptionId)
        {
            var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
            return RedirectToAction("Index");
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

    public class CustomerSubscriptionViewModel
    {
        public string UserName { get; set; }
        public List<SubscriptionViewModel> Subscriptions { get; set; }
        public string CardNumber { get; set; }
        public string Cvc { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public int DonationId { get; set; }
    }
}