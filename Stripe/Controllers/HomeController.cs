using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Controllers
{
    public class HomeController : Controller
    {
        const string SessionKey = "sessionKey";
        private readonly IBillingCycle _billingCycle;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(IBillingCycle billingCycle, ApplicationDbContext dbContext)
        {
            _billingCycle = billingCycle;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var model = new DonationViewModel
            {
                DonationCycles = _billingCycle
                    .GetCycles()
                    .Select(b => new SelectListItem
                    {
                        Text = b.Key.ToString(),
                        Value = b.Value.ToString()
                    }).ToList()
            };
            return View(model);
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

        public IActionResult Create()
        {
            if (User.Identity.IsAuthenticated)
            {
                var value = HttpContext.Session.GetString(SessionKey);
                if (!string.IsNullOrEmpty(value))
                {
                    var model = JsonConvert.DeserializeObject<Donation>(value);
                    _dbContext.Donations.Add(model);
                    _dbContext.SaveChanges();
                    return RedirectToAction("Index", "Billing", new { Id = model.Id });
                }
            }
            return RedirectToAction("Login", "Account", new { returnUrl = Request.Path });
        }


        [HttpPost]
        public IActionResult Create(DonationViewModel donation)
        {
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

            var model = new Donation
            {
                CycleId = donation.CycleId,
                DonationAmount = donation.DonationAmount,
                UserId = User.Identity.Name
            };
            _dbContext.Donations.Add(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "Billing", new { Id = 1/*model.Id*/});
        }
    }
}
