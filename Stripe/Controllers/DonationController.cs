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
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                var model = (DonationViewModel) donation;
                return View("Payment", model);
            }
            var plan = _donationService.GetPlan(donation);

            return View("Index");
        }

        public async Task<IActionResult> Index(int id)
        {
            var user = await GetCurrentUserAsync();
            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var customer = new StripeCustomerCreateOptions
                {
                    Email = "{user.email}",
                    Description = "{user.email} {user.Id}",
                    PlanId = "planId"
                };

                var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);

                var stripeCustomer = customerService.Create(customer);
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

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }
}