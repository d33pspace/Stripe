using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Stripe.Models
{
    public class DonationViewModel
    {
        public int Id { get; set; }

        public string CycleId { get; set; }

        public double? DonationAmount { get; set; }

        public List<SelectListItem> DonationCycles { get; set; }

        public int SelectedAmount { set; get; }

        public List<DonationOption> DonationOptions { get; set; }

        public DonationViewModel()
        {
            DonationOptions = new List<DonationOption>
            {
                new DonationOption {Id = 1, Amount = 18, Reason = "to provide one day of showers, laundry and care for five people."},
                new DonationOption {Id = 2, Amount = 63, Reason = "to provide a week of shelter and training for one person."},
                new DonationOption {Id = 3, Amount = 200, Reason = "towards shower renovations or the purchase of a new van."},
                new DonationOption {Id = 4, Amount = 0, Reason = "to help as many people as possible today!", IsCustom = true},
            };
        }

        public double GetAmount()
        {
            if (DonationAmount != null && DonationAmount > 0)
                return DonationAmount.Value;

            if (SelectedAmount > 0)
                return DonationOptions[SelectedAmount - 1].Amount;

            return 0.0;
        }

        public string GetDescription()
        {
            if (SelectedAmount == 0)
                return $"{DonationAmount} {DonationOptions[3].Reason}";
            return DonationOptions[SelectedAmount - 1].Description;
        }
    }

    public class DonationOption
    {
        public int Id { get; set; }

        public double Amount { get; set; }

        public string Reason { get; set; }

        public string Description => $"{Amount} {Reason}";

        public bool IsCustom { get; set; }
    }
}