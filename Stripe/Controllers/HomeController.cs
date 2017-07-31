using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBillingCycle _billingCycle;

        public HomeController(IBillingCycle billingCycle)
        {
            _billingCycle = billingCycle;
        }

        public IActionResult Index()
        {
            var model = new Donation
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

        [HttpPost]
        public IActionResult Create(Donation donation)
        {
            // Todo: Advance to next page
            return RedirectToAction("Index");
        }
    }
}
