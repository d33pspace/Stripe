using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    public class DonationController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationController(
            UserManager<ApplicationUser> userManager, 
            IDonationService donationService)
        {
            _userManager = userManager;
            _donationService = donationService;
        }

        public async Task<IActionResult> Payment(int id)
        {
            var user = await GetCurrentUserAsync();
            var payment = new PaymentViewViewModel();
            var donation = _donationService.GetById(id);
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