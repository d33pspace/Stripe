using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BillingController(
            UserManager<ApplicationUser> userManager, 
            IBillingService billingService)
        {
            _userManager = userManager;
            _billingService = billingService;
        }

        public async Task<IActionResult> Payment(int id)
        {
            var user = await GetCurrentUserAsync();
            var payment = new PaymentViewViewModel();
            var donation = _billingService.GetById(id);
            //if(donation.CycleId == )
            //payment.Subscriptions = user.Subscriptions.Select(s => new SubscriptionViewModel()).ToList();


            // Get Card types 

            return View(donation);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }
}