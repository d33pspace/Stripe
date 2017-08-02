using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Stripe.Models
{
    public class DonationViewModel
    {
        public int Id { get; set; }

        public string CycleId { get; set; }

        public double DonationAmount { get; set; }

        public List<SelectListItem> DonationCycles { get; set; }

        public DonationViewModel()
        {
            DonationCycles = new List<SelectListItem>();
        }
    }
}