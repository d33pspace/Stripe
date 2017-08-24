using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    [Authorize]
    public class DonationAlipayController : Controller
    {
        private const string DonationCaption = "Renewal Center donation";
        private readonly IDonationService _donationService;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationAlipayController(
            UserManager<ApplicationUser> userManager,
            IDonationService donationService,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _donationService = donationService;
            _stripeSettings = stripeSettings;
        }

        [Route("DonationAlipay/Payment")]
        [HttpGet]
        public ActionResult payment()
        {
            return RedirectToAction("index", "home");
        }

        [Route("DonationAlipay/Payment/{id}")]
        public async Task<IActionResult> Payment(int id)
        {
            var user = await GetCurrentUserAsync();
            var donation = _donationService.GetById(id);
            var detail = (DonationViewModel)donation;
            detail.DonationOptions = _donationService.DonationOptions;

            // Check for existing customer
            // edit = 1 means user wants to edit the credit card information
            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var CustomerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);

                StripeCustomer objStripeCustomer = null; try { CustomerService.Get(user.StripeCustomerId); } catch { }
                StripeCard objStripeCard = null;

                try
                {
                    if (objStripeCustomer.Sources != null && objStripeCustomer.Sources.TotalCount > 0 && objStripeCustomer.Sources.Data.Any())
                    {
                        objStripeCard = objStripeCustomer.Sources.Data.FirstOrDefault().Card;
                    }
                }
                catch { }

                if (objStripeCard != null && !string.IsNullOrEmpty(objStripeCard.Id))
                {
                    var objCustomerRePaymentViewModel = new CustomerRePaymentViewModel
                    {
                        Name = user.FullName,
                        AddressLine1 = user.AddressLine1,
                        AddressLine2 = user.AddressLine2,
                        City = user.City,
                        State = user.State,
                        Country = user.Country,
                        Zip = user.Zip,
                        DonationId = donation.Id,
                        Description = detail.GetDescription(),
                        Frequency = detail.GetCycle(),
                        Amount = detail.GetDisplayAmount(),
                        Last4Digit = objStripeCard.Last4,
                        CardId = objStripeCard.Id,
                        Currency = (objStripeCustomer.Currency + "").ToUpper()
                    };

                    return View("RePayment", objCustomerRePaymentViewModel);
                }
            }

            var model = new CustomerPaymentViewModel
            {
                Name = user.FullName,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                City = user.City,
                State = user.State,
                Country = user.Country,
                Zip = user.Zip,
                DonationId = donation.Id,
                Description = detail.GetDescription(),
                Frequency = detail.GetCycle(),
                Amount = detail.GetDisplayAmount()
            };

            return View("Payment", model);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(CustomerPaymentViewModel payment)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (!ModelState.IsValid)
                {
                    return View(payment);
                }

                var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);
                var donation = _donationService.GetById(payment.DonationId);
                var customer = new StripeCustomerCreateOptions();
                // Construct payment

                customer = new StripeCustomerCreateOptions
                {
                    Email = user.Email,
                    Description = $"{user.Email} {user.Id}",
                    SourceCard = new SourceCard
                    {
                        Name = payment.Name,
                        Number = payment.CardNumber,
                        Cvc = payment.Cvc,
                        ExpirationMonth = payment.ExpiryMonth,
                        ExpirationYear = payment.ExpiryYear,
                        StatementDescriptor = _stripeSettings.Value.StatementDescriptor,
                        Description = DonationCaption,
                        AddressLine1 = payment.AddressLine1,
                        AddressLine2 = payment.AddressLine2,
                        AddressCity = payment.City,
                        AddressState = payment.State,
                        AddressCountry = payment.Country,
                        AddressZip = payment.Zip
                    }
                };
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var stripeCustomer = customerService.Create(customer);
                    user.StripeCustomerId = stripeCustomer.Id;
                }
                else
                {
                    //Check for existing credit card, if new credit card number is same as exiting credit card then we delete the existing
                    //Credit card information so new card gets generated automatically as default card.
                    try
                    {
                        var ExistingCustomer = customerService.Get(user.StripeCustomerId);
                        if (ExistingCustomer.Sources != null && ExistingCustomer.Sources.TotalCount > 0 && ExistingCustomer.Sources.Data.Any())
                        {
                            var cardService = new StripeCardService(_stripeSettings.Value.SecretKey);
                            foreach (var cardSource in ExistingCustomer.Sources.Data)
                            {
                                cardService.Delete(user.StripeCustomerId, cardSource.Card.Id);
                            }
                        }
                    }
                    catch (Exception exSub)
                    {
                    }

                    var customerUpdate = new StripeCustomerUpdateOptions
                    {
                        SourceCard = new SourceCard
                        {
                            Name = payment.Name,
                            Number = payment.CardNumber,
                            Cvc = payment.Cvc,
                            ExpirationMonth = payment.ExpiryMonth,
                            ExpirationYear = payment.ExpiryYear,
                            StatementDescriptor = _stripeSettings.Value.StatementDescriptor,
                            Description = DonationCaption,
                            AddressLine1 = payment.AddressLine1,
                            AddressLine2 = payment.AddressLine2,
                            AddressCity = payment.City,
                            AddressState = payment.State,
                            AddressCountry = payment.Country,
                            AddressZip = payment.Zip
                        }
                    };
                    try
                    {
                        var stripeCustomer = customerService.Update(user.StripeCustomerId, customerUpdate);
                        user.StripeCustomerId = stripeCustomer.Id;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var stripeCustomer = customerService.Create(customer);
                            user.StripeCustomerId = stripeCustomer.Id;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                user.FullName = payment.Name;
                user.AddressLine1 = payment.AddressLine1;
                user.AddressLine2 = payment.AddressLine2;
                user.City = payment.City;
                user.State = payment.State;
                user.Country = payment.Country;
                user.Zip = payment.Zip;
                await _userManager.UpdateAsync(user);

                //-- Start Alipay Code
                var model = (DonationViewModel)donation;
                model.DonationOptions = _donationService.DonationOptions;

                var aliPayURL = string.Empty;
                if (string.IsNullOrEmpty(payment.Currency))
                    payment.Currency = "usd";

                donation.DonationAmount = payment.Amount;
                aliPayURL = aliPayAuthentication(donation, payment.Currency, user.Email);
                if (!string.IsNullOrEmpty(aliPayURL))
                    return Redirect(aliPayURL);
            }
            catch (StripeException sex)
            {
                ModelState.AddModelError("error", sex.Message);
                return View(payment);
            }
            catch (Exception ex)
            {
                return View("Error");
            }

            return View("Error");
        }

        [HttpGet]
        public async Task<IActionResult> getAlipayResopnce()
        {
            var source = Request.Query["source"];
            var livemode = Request.Query["livemode"];
            var client_secret = Request.Query["client_secret"];
            var dnId = Request.Query["dnId"];
            var currId = Request.Query["currId"];



            var user = await GetCurrentUserAsync();
            var donation = _donationService.GetById(Convert.ToInt32(dnId));
            var detail = (DonationViewModel)donation;
            detail.DonationOptions = _donationService.DonationOptions;

            //Add to existing subscriptions and charge
            donation.currency = currId;
            if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
            {
                StripeConfiguration.SetApiKey(_stripeSettings.Value.SecretKey);
                var chargeOptions = new StripeChargeCreateOptions()
                {
                    Amount = detail.GetAmount(),
                    Currency = donation.currency,
                    Description =  detail.GetDescription(),
                    SourceTokenOrExistingSourceId = source,
                    CustomerId = user.StripeCustomerId
                };
                var chargeService = new StripeChargeService();
                StripeCharge charge = chargeService.Create(chargeOptions);
                if (charge.Status == "succeeded")
                {
                    var completedMessage = new CompletedViewModel
                    {
                        Message = $"Thank you donating {chargeOptions.Amount} for the payment {chargeOptions.Description} "
                    };
                    return View("Thanks", completedMessage);
                }
                return View("Error");
            }

            var plan = _donationService.GetOrCreatePlan(donation);
            var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
            var result = subscriptionService.Create(user.StripeCustomerId, plan.Id);
            if (result != null)
            {
                var completedMessage = new CompletedViewModel
                {
                    Message = $"You have added a subscription {result.StripePlan.Name} for this donation",
                    HasSubscriptions = true
                };
                return View("Thanks", completedMessage);
            }
            return View("Error");
        }


        [Route("DonationAlipay/Payment/{id}/{edit?}")]
        public async Task<IActionResult> Payment(int id, int edit)
        {
            var user = await GetCurrentUserAsync();
            var donation = _donationService.GetById(id);
            var detail = (DonationViewModel)donation;
            detail.DonationOptions = _donationService.DonationOptions;

            var model = new CustomerPaymentViewModel
            {
                Name = user.FullName,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                City = user.City,
                State = user.State,
                Country = user.Country,
                Zip = user.Zip,
                DonationId = donation.Id,
                Description = detail.GetDescription(),
                Frequency = detail.GetCycle(),
                Amount = detail.GetDisplayAmount()
            };

            return View("Payment", model);
        }

        [HttpPost]
        public async Task<IActionResult> RePayment(CustomerRePaymentViewModel payment)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                if (!ModelState.IsValid)
                {
                    return View(payment);
                }

                var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);
                var donation = _donationService.GetById(payment.DonationId);

                user.FullName = payment.Name;
                user.AddressLine1 = payment.AddressLine1;
                user.AddressLine2 = payment.AddressLine2;
                user.City = payment.City;
                user.State = payment.State;
                user.Country = payment.Country;
                user.Zip = payment.Zip;
                await _userManager.UpdateAsync(user);

                // Add customer to Stripe
                if (EnumInfo<PaymentCycle>.GetValue(donation.CycleId) == PaymentCycle.OneOff)
                {
                    var model = (DonationViewModel)donation;
                    model.DonationOptions = _donationService.DonationOptions;

                    var charges = new StripeChargeService(_stripeSettings.Value.SecretKey);

                    // Charge the customer
                    var charge = charges.Create(new StripeChargeCreateOptions
                    {
                        Amount = model.GetAmount(),
                        Description = DonationCaption,
                        Currency = payment.Currency.ToLower(),
                        CustomerId = user.StripeCustomerId,
                        //ReceiptEmail = user.Email,
                        StatementDescriptor = _stripeSettings.Value.StatementDescriptor,
                    });

                    if (charge.Paid)
                    {
                        var completedMessage = new CompletedViewModel
                        {
                            Message = $"Thank you donating {model.GetDisplayAmount()} for the payment {model.GetDescription()} "
                        };
                        return View("Thanks", completedMessage);
                    }
                    return View("Error");
                }
                donation.currency = payment.Currency;
                // Add to existing subscriptions and charge 
                var plan = _donationService.GetOrCreatePlan(donation);

                var subscriptionService = new StripeSubscriptionService(_stripeSettings.Value.SecretKey);
                var result = subscriptionService.Create(user.StripeCustomerId, plan.Id);
                if (result != null)
                {
                    var completedMessage = new CompletedViewModel
                    {
                        Message = $"You have added a subscription {result.StripePlan.Name} for this donation",
                        HasSubscriptions = true
                    };
                    return View("Thanks", completedMessage);
                }
            }
            catch (StripeException sex)
            {
                ModelState.AddModelError("error", sex.Message);
                return View(payment);
            }
            catch (Exception ex)
            {
                return View("Error");
            }

            return View("Error");
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
            //Source src = new Source();
            //src.Type = SourceType.
        }

        public string aliPayAuthentication(DonationViewModel donation, string currency, string Email)
        {
            var url = string.Empty;
            try
            {
                StripeConfiguration.SetApiKey(_stripeSettings.Value.SecretKey);
                var sourceOptions = new StripeSourceCreateOptions()
                {
                    Type = StripeSourceType.Alipay,
                    Amount = donation.GetAmount(),
                    Currency = currency,
                    RedirectReturnUrl = "https://localhost:44341/DonationAlipay/getAlipayResopnce?dnId=" + donation.Id + "&currId=" + currency,
                    Owner = new StripeSourceOwner()
                    {
                        Email = Email
                    }
                };
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
    }
}