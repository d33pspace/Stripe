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
        private readonly IDonationService _donationService;
        const string SessionKey = "sessionKey";

        public HomeController(UserManager<ApplicationUser> userManager, IDonationService donationService)
        {
            _userManager = userManager;
            _donationService = donationService;
        }

        public IActionResult Index()
        {
            var model = new DonationViewModel
            {
                DonationCycles = GetDonationCycles
            };
            return View(model);
        }

        /// <summary>
        /// Executed when the user has now been authenticated and the user wants to access their subscriptions
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            if (User.Identity.IsAuthenticated)
            {
                var value = HttpContext.Session.GetString(SessionKey);
                if (!string.IsNullOrEmpty(value))
                {
                    var user = await GetCurrentUserAsync();
                    var model = JsonConvert.DeserializeObject<Donation>(value);
                    model.User = user;
                    model.UserId = user.Id;

                    _donationService.Save(model);
                    return RedirectToAction("Payment", "Donation", new { Id = model.Id });
                }
            }
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
        }


        [HttpPost]
        public async Task<IActionResult> Create(DonationViewModel donation)
        {
            if (Math.Abs(donation.GetAmount()) < 1)
            {
                ModelState.AddModelError("amount", "Donation amount cannot be zero or less");
            }

            if (donation.CycleId == "Please select one") //Could be better
            {
                ModelState.AddModelError("cycle", "Donation cycle is required");
            }

            if (!ModelState.IsValid)
            {
                donation.DonationCycles = GetDonationCycles;
                return View("Index", donation);
            }

            Donation model;
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                model = new Donation
                {
                    CycleId = donation.CycleId,
                    DonationAmount = donation.DonationAmount,
                    SelectedAmount = donation.SelectedAmount,
                    TransactionDate = DateTime.Now
                };
                _donationService.Save(model);
                return RedirectToAction("Payment", "Donation", new { Id = model.Id });
            }

            // If user is not authenticated, lets save the details on the session cache and we get them after authentication
            if (!User.Identity.IsAuthenticated)
            {
                var value = HttpContext.Session.GetString(SessionKey);
                if (string.IsNullOrEmpty(value))
                {
                    var donationView = JsonConvert.SerializeObject(donation);
                    HttpContext.Session.SetString(SessionKey, donationView);
                }

                // This will redirect to "Create" action method when the user has been redirected after authentication
                // as they will be required to authenticate before they can use their subscriptions
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }

            // User has logged in
            var user = await GetCurrentUserAsync();
            model = new Donation
            {
                CycleId = donation.CycleId,
                DonationAmount = donation.DonationAmount,
                SelectedAmount = donation.SelectedAmount,
                TransactionDate = DateTime.Now,
                User = user,
                UserId = user.Id
            };
            _donationService.Save(model);
            return RedirectToAction("Index", "Donation", new { Id = model.Id });
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

        private List<SelectListItem> GetDonationCycles => _donationService
            .GetCycles()
            .Select(b => new SelectListItem
            {
                Value = ((int)b.Key).ToString(),
                Text = b.Value
            }).ToList();

        private Task<ApplicationUser> GetCurrentUserAsync() => 
            _userManager.GetUserAsync(HttpContext.User);
    }

}
