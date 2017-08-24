using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe.Services;

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

        public string PaymentGatway { get; set; }


        public DonationViewModel()
        {
        }

        public DonationViewModel(List<DonationListOption> donationOptions)
        {
            DonationOptions = donationOptions;
        }

        public int GetAmount()
        {
            if (DonationAmount > 0)
                return DonationAmount.Value * StripeMultiplier;

            if (SelectedAmount > 0)
                return DonationOptions[SelectedAmount - 1].Amount * StripeMultiplier;

            return 0;
        }

        public string GetCycle()
        {
            var pc = EnumInfo<PaymentCycle>.GetValue(this.CycleId);
            return EnumInfo<PaymentCycle>.GetDescription(pc);
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
            return $"{DonationOptions[SelectedAmount - 1].Amount} {DonationOptions[SelectedAmount - 1].Reason}";
        }

        public string GetDescription()
        {
            return DonationOptions[SelectedAmount - 1].Reason;
        }

        public static implicit operator DonationViewModel(Donation donation)
        {
            return new DonationViewModel(new List<DonationListOption>())
            {
                Id = donation.Id,
                CycleId = donation.CycleId,
                DonationAmount = donation.DonationAmount,
                SelectedAmount = donation.SelectedAmount,
            };
        }
    }
}