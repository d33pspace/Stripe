using Microsoft.AspNetCore.Mvc;
using Stripe.Services;

namespace Stripe.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        public IActionResult Index(int id)
        {
            // Get Card types 
            var donation = _billingService.GetById(id);
            return View(donation);
        }
    }
}