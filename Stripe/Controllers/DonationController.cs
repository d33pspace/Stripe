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
    public class DonationController : Controller
    {
        private const string DonationCaption = "Renewal Center donation";
        private readonly IDonationService _donationService;
        private readonly IOptions<StripeSettings> _stripeSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public DonationController(
            UserManager<ApplicationUser> userManager,
            IDonationService donationService,
            IOptions<StripeSettings> stripeSettings)
        {
            _userManager = userManager;
            _donationService = donationService;
            _stripeSettings = stripeSettings;
        }

        [Route("Donation/Payment")]
        [HttpGet]
        public ActionResult payment()
        {
            return RedirectToAction("index", "home");
        }

        [Route("Donation/Payment/{id}")]
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
                StripeCustomer objStripeCustomer = CustomerService.Get(user.StripeCustomerId);
                StripeCard objStripeCard = null;

                if (objStripeCustomer.Sources != null && objStripeCustomer.Sources.TotalCount > 0 && objStripeCustomer.Sources.Data.Any())
                {
                    objStripeCard = objStripeCustomer.Sources.Data.FirstOrDefault().Card;
                }

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
                        CardId = objStripeCard.Id
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

                // Construct payment
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var customer = new StripeCustomerCreateOptions
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

                    var stripeCustomer = customerService.Create(customer);
                    user.StripeCustomerId = stripeCustomer.Id;
                }
                else
                {
                    //Check for existing credit card, if new credit card number is same as exiting credit card then we delete the existing
                    //Credit card information so new card gets generated automatically as default card.
                    var ExistingCustomer = customerService.Get(user.StripeCustomerId);
                    if (ExistingCustomer.Sources != null && ExistingCustomer.Sources.TotalCount > 0 && ExistingCustomer.Sources.Data.Any())
                    {
                        var cardService = new StripeCardService(_stripeSettings.Value.SecretKey);
                        foreach (var cardSource in ExistingCustomer.Sources.Data)
                        {
                            cardService.Delete(user.StripeCustomerId, cardSource.Card.Id);
                        }
                    }

                    var customer = new StripeCustomerUpdateOptions
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

                    var stripeCustomer = customerService.Update(user.StripeCustomerId, customer);
                    user.StripeCustomerId = stripeCustomer.Id;
                }

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
                        Currency = "usd",
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

        [Route("Donation/Payment/{id}/{edit?}")]
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
                        Currency = "usd",
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
        }

    }

}