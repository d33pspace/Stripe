using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBillingService _billingService;
        const string SessionKey = "sessionKey";

        public HomeController(UserManager<ApplicationUser> userManager, IBillingService billingService)
        {
            _userManager = userManager;
            _billingService = billingService;
        }

        public IActionResult Index()
        {
            var donationCycles = _billingService
                .GetCycles()
                .Select(b => new SelectListItem
                {
                    Text = b.Key.ToString(),
                    Value = b.Value.ToString()
                }).ToList();

            var model = new DonationViewModel
            {
                DonationCycles = donationCycles
            };
            return View(model);
        }

        public IActionResult Create()
        {
            if (User.Identity.IsAuthenticated)
            {
                var value = HttpContext.Session.GetString(SessionKey);
                if (!string.IsNullOrEmpty(value))
                {
                    var model = JsonConvert.DeserializeObject<Donation>(value);
                    _billingService.Add(model);
                    return RedirectToAction("Index", "Billing", new { Id = model.Id });
                }
            }
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
        }


        [HttpPost]
        public async Task<IActionResult> Create(DonationViewModel donation)
        {
            // If user is not authenticated, lets save the details on the session cache and we get them after authentication
            if (!User.Identity.IsAuthenticated)
            {
                var value = HttpContext.Session.GetString(SessionKey);
                if (string.IsNullOrEmpty(value))
                {
                    var donationView = JsonConvert.SerializeObject(donation);
                    HttpContext.Session.SetString(SessionKey, donationView);
                }
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }

            // User is authentication
            var user = await GetCurrentUserAsync();
            var model = new Donation
            {
                CycleId = donation.CycleId,
                DonationAmount = donation.DonationAmount,
                UserId = user.Id,
                User = user,
                TransactionDate = DateTime.Now
            };
            _billingService.Add(model);

            return RedirectToAction("Index", "Billing", new { Id = model.Id });
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
    }

}
