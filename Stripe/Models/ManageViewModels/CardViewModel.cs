using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Stripe.Models.ManageViewModels
{
    public class CardViewModel
    {
        public string cardId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"\d{3}", ErrorMessage = "Invalid CVC number")]
        public string Cvc { get; set; }

        [Range(1, 12, ErrorMessage = "Invalid month")]
        public int ExpiryMonth { get; set; }

        [Range(17, 30, ErrorMessage = "Invalid year")]
        public int ExpiryYear { get; set; }
    }
}
