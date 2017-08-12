using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Controllers;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.ViewComponents
{
    public class SubscriptionsViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<StripeSettings> _stripeSettings;

        public SubscriptionsViewComponent(
            UserManager<ApplicationUser> userManager,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _stripeSettings = stripeSettings;
        }

        public IViewComponentResult Invoke(int numberOfItems)
        {
            var user = GetCurrentUserAsync();

            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var customerService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
                var subscriptions = customerService.List(user.StripeCustomerId);

                var customerSubscription = new CustomerPaymentViewModel
                {
                    UserName = user.Email,
                    Subscriptions = subscriptions.Select(s => new CustomerSubscriptionViewModel
                    {
                        Id = s.Id,
                        Name = s.StripePlan.Name,
                        Amount = s.StripePlan.Amount,
                        Currency = s.StripePlan.Currency,
                        Status = s.Status
                    }).ToList()
                };
                return View("View", customerSubscription);
            }
            var subscription = new CustomerPaymentViewModel
            {
                UserName = user.Email,
                Subscriptions = new List<CustomerSubscriptionViewModel>()
            };
            return View(subscription);
        }

        private ApplicationUser GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User).Result;
        }
    }
}