using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Data;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        public BillingController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICardProvider cardProvider,
            SubscriptionsFacade subscriptionsFacade,
            InvoiceDataService<ApplicationDbContext, ApplicationUser> invoiceDataService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cardProvider = cardProvider;
            _subscriptionsFacade = subscriptionsFacade;
            _invoiceDataService = invoiceDataService;
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private readonly UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ICardProvider _cardProvider;
        private readonly SubscriptionsFacade _subscriptionsFacade;
        private readonly InvoiceDataService<ApplicationDbContext, ApplicationUser> _invoiceDataService;


        public async Task<ViewResult> Index()
        {
            var user = await GetCurrentUserAsync();
            ViewBag.Subscriptions = await _subscriptionsFacade.UserActiveSubscriptionsAsync(user.Id);
            ViewBag.PaymentDetails = await _subscriptionsFacade.DefaultCreditCard(user.Id);
            //ViewBag.Invoices = await InvoiceDataService.UserInvoicesAsync(user.Id);

            return View();
        }

        public async Task<ViewResult> ChangeSubscription()
        {
            var user = await GetCurrentUserAsync();
            var currentSubscription = (await _subscriptionsFacade.UserActiveSubscriptionsAsync(user.Id)).FirstOrDefault();

            var model = new ChangeSubscriptionViewModel
            {
                //SubscriptionPlans = await SubscriptionPlansFacade.GetAllAsync(),
                CurrentSubscription = currentSubscription != null ? currentSubscription.SubscriptionPlan.Id : string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeSubscription(ChangeSubscriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = await GetCurrentUserAsync();
                var user = await _userManager.FindByIdAsync(userId.Id);
                await _subscriptionsFacade.UpdateSubscriptionAsync(user.Id, user.StripeCustomerId, model.NewPlan);

                // TempData.Add("flash", new FlashSuccessViewModel("Your subscription plan has been updated."));
            }
            else
            {
                // TempData.Add("flash", new FlashSuccessViewModel("Sorry, there was an error updating your plan, try again or contact support."));
            }

            return RedirectToAction("Index");
        }

        public ActionResult CancelSubscription(int id)
        {
            return View(new CancelSubscriptionViewModel { Id = id });
        }

        [HttpPost]
        public async Task<ActionResult> CancelSubscription(CancelSubscriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();

                var currentSubscription = (await _subscriptionsFacade.UserActiveSubscriptionsAsync(user.Id)).FirstOrDefault();

                DateTime? endDate; // Because we are passing CancelAtTheEndOfPeriod to EndSubscription, we get the date when the subscription will be cancelled
                if (currentSubscription != null &&
                    (endDate = await _subscriptionsFacade.EndSubscriptionAsync(currentSubscription.Id, user, true, model.Reason)) != null)
                {
                    // TempData.Add("flash", new FlashSuccessViewModel("Your subscription has been cancelled."));
                }
                else
                {
                    // TempData.Add("flash", new FlashDangerViewModel("Sorry, there was a problem cancelling your subscription."));
                }

                return RedirectToAction("Index", "Billing");
            }

            return View(model);
        }

        public async Task<ActionResult> ReActivateSubscription()
        {
            var user = await GetCurrentUserAsync();

            var currentSubscription = (await _subscriptionsFacade.UserActiveSubscriptionsAsync(user.Id)).FirstOrDefault();

            if (currentSubscription != null &&
                await _subscriptionsFacade.UpdateSubscriptionAsync(user.Id, user.StripeCustomerId, currentSubscription.SubscriptionPlanId))
            {
                // TempData.Add("flash", new FlashSuccessViewModel("Your subscription plan has been re-activated."));
            }
            else
            {
                // TempData.Add("flash", new FlashDangerViewModel("Ooops! There was a problem re-activating your subscription. Please, try again."));
            }

            return RedirectToAction("Index");
        }

        public ActionResult AddCreditCard()
        {
            return View(new CreditCardViewModel
            {
                CreditCard = new CreditCard()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddCreditCard(CreditCardViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();

                if (user != null)
                {
                   // await _subscriptionsFacade.SubscribeUserAsync();
                }

                await _cardProvider.AddAsync(user, model.CreditCard);

                // TempData.Add("flash", new FlashSuccessViewModel("Your credit card has been saved successfully."));

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<ActionResult> ChangeCreditCard(int? id)
        {
            var user = await GetCurrentUserAsync();

            if (id == null)
            {
                return null;
            }

            var model = new CreditCardViewModel
            {
                CreditCard = await _cardProvider.FindAsync(user.Id, id)
            };

            // TODO: Substitute null
            // If the card doesn't exist or doesn't belong the logged in user
            if (model.CreditCard == null || model.CreditCard.UserId != user.Id)
            {
                return null;
            }
            model.CreditCard.ClearCreditCardDetails();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeCreditCard(CreditCardViewModel model)
        {
            var user = await GetCurrentUserAsync();

            if (ModelState.IsValid && await _cardProvider.CardBelongToUser(model.CreditCard.Id, user.Id))
            {
                await _cardProvider.UpdateAsync(user, model.CreditCard);

                // TempData.Add("flash", new FlashSuccessViewModel("Your credit card has been updated successfully."));

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ViewResult BillingAddress()
        {
            // TODO: Get Billing address from your model
            var model = new BillingAddress();

            return View(model);
        }

        [HttpPost]
        public ActionResult BillingAddress(BillingAddress model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Call your service to save the billing address


                // TempData.Add("flash", new FlashSuccessViewModel("Your billing address has been saved."));

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public async Task<ViewResult> Invoice(int id)
        {
            var user = GetCurrentUserAsync();
            // TODO: Fix this 
            var invoice = await _invoiceDataService.UserInvoiceAsync(user.Id.ToString(), id);
            return View(invoice);
        }

        public async Task<ActionResult> DeleteAccount()
        {
            var user = await GetCurrentUserAsync();

            // Delete User
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index", "Home");
        }
    }
}