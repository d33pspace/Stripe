using System;
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
    [AllowAnonymous]
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly ICardService _cardService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationController(
            UserManager<ApplicationUser> userManager, 
            IDonationService donationService,
            ICardService cardService,
            ISubscriptionService subscriptionService,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _donationService = donationService;
            _cardService = cardService;
            _subscriptionService = subscriptionService;
            _stripeSettings = stripeSettings;
        }

        public async Task<IActionResult> Payment(int id)
        {
            var donation = _donationService.GetById(id);
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                var model = (DonationViewModel) donation;
                return View("Payment", model);
            }

            // Lets search for subscriptions since this user wants monthly/weekly payments
            var user = await GetCurrentUserAsync();
            var subscriptionService = new StripeSubscriptionService();
            StripeSubscriptionListOptions lo = new StripeSubscriptionListOptions
            {
                CustomerId = user.StripeCustomerId
            };

            ViewBag.Subscriptions = subscriptionService.List(lo);
            return View("Subscriptions", new {Id = id});
        }

        public async Task<IActionResult> Confirm(string planId, string stripeToken)
        {
            var user = await GetCurrentUserAsync();
            //var planId = "$85"

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var customer = new StripeCustomerCreateOptions
                {
                    Email = "{user.email}",
                    Description = "{user.email} {user.Id}",
                    PlanId = planId,
                    SourceToken = stripeToken
                };

                var customerService = new StripeCustomerService();

                StripeCustomer stripeCustomer = customerService.Create(customer);
                user.StripeCustomerId = stripeCustomer.Id;
                stripeCustomer.Subscriptions.Data.ForEach(s => SaveSubscription(s, user));
                await _userManager.UpdateAsync(user);
            }
            else
            {
                var subscriptionService = new StripeSubscriptionService();
                var stripeSubscription = subscriptionService.Create(user.StripeCustomerId);
                await SaveSubscription(stripeSubscription, user);
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Payment");
        }

        private Task SaveSubscription(StripeSubscription stripeSubscription, ApplicationUser user)
        {
            return null;
        }


        [HttpPost]
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

        [Authorize]
        public async Task<IActionResult> Subscriptions(int? id)
        {

            var user = await GetCurrentUserAsync();
            var payment = new PaymentViewViewModel();
            //payment.Subscriptions = user.Subscriptions.Select(s => new SubscriptionViewModel()).ToList();
            return View();
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }
}