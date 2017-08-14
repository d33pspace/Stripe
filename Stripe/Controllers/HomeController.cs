using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    public class HomeController : BaseController
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
            var model = new DonationViewModel(_donationService.DonationOptions)
            {
                DonationCycles = GetDonationCycles
            };
            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var value = HttpContext.Session.GetString(SessionKey);
            if (!string.IsNullOrEmpty(value))
            {
                var model = JsonConvert.DeserializeObject<Donation>(value);
                return RedirectToAction("Payment", "Donation", new { Id = model.Id });
            }
            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> Create(DonationViewModel donation)
        {
            donation.DonationOptions = _donationService.DonationOptions;

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

            var model = new Donation
            {
                CycleId = donation.CycleId,
                DonationAmount = donation.DonationAmount,
                SelectedAmount = donation.SelectedAmount,
                TransactionDate = DateTime.Now
            };
            _donationService.Save(model);

            // If user is not authenticated, lets save the details on the session cache and we get them after authentication
            if (!User.Identity.IsAuthenticated)
            {
                var value = HttpContext.Session.GetString(SessionKey);
                if (string.IsNullOrEmpty(value))
                {
                    var donationJson = JsonConvert.SerializeObject(model);
                    HttpContext.Session.SetString(SessionKey, donationJson);
                }
                return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
            }
            return RedirectToAction("Payment", "Donation", new { id = model.Id });
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
