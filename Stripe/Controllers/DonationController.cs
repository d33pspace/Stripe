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

        public IActionResult Payment(int id)
        {
            var donation = _donationService.GetById(id);
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                var model = (DonationViewModel) donation;
                return View("Payment", model);
            }
            return View("Subscriptions", new {Id = id});
        }

        [HttpPost]
        public IActionResult Charge(string stripeEmail, string stripeToken)
        {
            _cardService.Charge(stripeEmail, stripeToken);
            return View();
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