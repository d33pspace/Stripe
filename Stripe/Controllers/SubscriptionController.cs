using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDonationService _donationService;
        private readonly IOptions<StripeSettings> _stripeSettings;

        public SubscriptionController(UserManager<ApplicationUser> userManager,
            IDonationService donationService,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _donationService = donationService;
            _stripeSettings = stripeSettings;
        }
        public IActionResult Delete(string subscriptionId)
        {
            var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
            var result = subscriptionService.Cancel(subscriptionId);

            ViewBag.Message = "You have sccessfully deleted subscription";
            return RedirectToAction("Index", "Manage");
        }

    }
}