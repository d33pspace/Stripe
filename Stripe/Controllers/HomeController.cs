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
using Microsoft.Extensions.Options;

namespace Stripe.Controllers
{
    public class HomeController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDonationService _donationService;
        private readonly IOptions<StripeSettings> _stripeSettings;
        const string SessionKey = "sessionKey";

        public HomeController(UserManager<ApplicationUser> userManager, IDonationService donationService, IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _donationService = donationService;
            _stripeSettings = stripeSettings;
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
    

        public string aliPayAuthentication(DonationViewModel donation)
        {
            var url = string.Empty;
            try
            {
                StripeConfiguration.SetApiKey(_stripeSettings.Value.SecretKey);
                var sourceOptions = new StripeSourceCreateOptions()
                {
                    //Type = StripeSourceType.Alipay,
                    Amount = 10122,
                    Currency = "usd",
                    RedirectReturnUrl = "https://localhost:44341/Home/getAlipayResopnce",
                    Owner = new StripeSourceOwner()
                    {
                        Email = "girishkolte20001@gmail.com"
                    },
                    Card = new StripeCreditCardOptions()
                    {
                        Name = "GIRISHK",
                        Number = "5105105105105100",
                        ExpirationMonth = 12,
                        ExpirationYear = 2022,
                        Cvc = "254"
                    }
                };
                ////"http://www.girishkolte.com/response",
                var sourceService = new StripeSourceService();
                StripeSource source = sourceService.Create(sourceOptions);

                if (source != null && source.Redirect != null)
                    url = source.Redirect.Url;
            }
            catch (Exception ex)
            {
            }


            return url;
        }


        [HttpPost]
        public async Task<IActionResult> Create(DonationViewModel donation)
        {
            donation.DonationOptions = _donationService.DonationOptions;

            if (donation.SelectedAmount == 0) //Could be better
            {
                ModelState.AddModelError("amount", "Select amount");
            }


            if (Math.Abs(donation.GetAmount()) < 1)
            {
                ModelState.AddModelError("amount", "Donation amount cannot be zero or less");
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
                currency = "",
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
            //if (donation.PaymentGatway == "stripe")
            //    return RedirectToAction("Payment", "Donation", new { id = model.Id });
            //else
            //    return RedirectToAction("Payment", "DonationAlipay", new { id = model.Id });
            //}

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
