using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Stripe.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public virtual DateTime? RegistrationDate { get; set; }

        public virtual DateTime? LastLoginTime { get; set; }

        public string StripeCustomerId { get; set; }

        public virtual ICollection<Subscription> Subscriptions { get; set; }

        public virtual IList<Invoice> Invoices { get; set; }

        public virtual IList<CreditCard> CreditCards { get; set; }          

        public string IPAddress { get; set; }

        public string IPAddressCountry { get; set; }

        public bool Delinquent { get; set; }

        public decimal LifetimeValue { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
//            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
//            return userIdentity;
            return null;
        }

        public bool HasUserDetails()
        {
            if (string.IsNullOrEmpty(FirstName) ||
                string.IsNullOrEmpty(LastName))
            {
                return false;
            }

            return true;
        }

        public bool HasPaymentDetails()
        {
            return CreditCards != null && CreditCards.Any();
        }
    }
}
