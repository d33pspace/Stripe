using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Stripe.Models
{
    public class DonationViewModel
    {
        public int Id { get; set; }

        public string CycleId { get; set; }

        public int? DonationAmount { get; set; }

        public List<SelectListItem> DonationCycles { get; set; }

        public int SelectedAmount { set; get; }

        public List<DonationListOption> DonationOptions { get; set; }

        private const int StripeMultiplier = 100;

        public DonationViewModel()
        {
            DonationOptions = new List<DonationListOption>
            {
                new DonationListOption {Id = 1, Amount = 18, Reason = "to provide one day of showers, laundry and care for five people."},
                new DonationListOption {Id = 2, Amount = 63, Reason = "to provide a week of shelter and training for one person."},
                new DonationListOption {Id = 3, Amount = 200, Reason = "towards shower renovations or the purchase of a new van."},
                new DonationListOption {Id = 4, Amount = 0, Reason = "to help as many people as possible today!", IsCustom = true},
            };
        }

        public int GetAmount()
        {
            if (DonationAmount > 0)
                return DonationAmount.Value * StripeMultiplier;

            if (SelectedAmount > 0)
                return DonationOptions[SelectedAmount - 1].Amount * StripeMultiplier;

            return 0;
        }

        public int GetDisplayAmount()
        {
            if (DonationAmount > 0)
                return DonationAmount.Value;

            if (SelectedAmount > 0)
                return DonationOptions[SelectedAmount - 1].Amount;

            return 0;
        }

        public string GetFullDescription()
        {
            if (SelectedAmount == 0)
                return $"{DonationAmount} {DonationOptions[3].Reason}";
            return $"{DonationOptions[SelectedAmount - 1].Amount} {DonationOptions[SelectedAmount - 1].Reason}";
        }

        public string GetDescription()
        {
            if (SelectedAmount == 0)
                return DonationOptions[3].Reason;
            return DonationOptions[SelectedAmount - 1].Reason;
        }

        public bool IsCustom()
        {
            return SelectedAmount == 3;
        }

        public static implicit operator DonationViewModel(Donation donation)
        {
            return new DonationViewModel
            {
                Id = donation.Id,
                CycleId = donation.CycleId,
                DonationAmount = donation.DonationAmount,
                SelectedAmount = donation.SelectedAmount,
            };
        }
    }
}