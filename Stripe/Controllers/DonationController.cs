﻿using System;
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

        public async Task<IActionResult> Payment(int id)
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
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Payment(CustomerPaymentViewModel payment)
        {
            var user = await GetCurrentUserAsync();
            // Can be better
            if ((payment.ExpiryYear + 2000) == DateTime.Now.Year && payment.ExpiryMonth <= DateTime.Now.Month)
                ModelState.AddModelError("expiredCard", "Expired card");

            if (!ModelState.IsValid)
            {
                return View(payment);
            }

            var customerService = new StripeCustomerService(_stripeSettings.Value.SecretKey);
            var donation = _donationService.GetById(payment.DonationId);

            // Construct payment
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

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                var stripeCustomer = customerService.Create(customer);
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
            return View("Error");
        }


        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }

}