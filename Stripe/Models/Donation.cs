using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Stripe.Models
{
    public class Donation
    {
        public int Id { get; set; }

        public string CycleId { get; set; }

        public double DonationAmount { get; set; }

        public List<SelectListItem> DonationCycles { get; set; }

        public Donation()
        {
            DonationCycles = new List<SelectListItem>();
        }
    }
}