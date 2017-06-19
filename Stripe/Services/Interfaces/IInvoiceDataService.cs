using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe.Models;

namespace Stripe.Services
{
    public interface IInvoiceDataService
    {
        Task<List<Invoice>> UserInvoicesAsync(string userId);

        Task<Invoice> UserInvoiceAsync(string userId, int invoiceId);

        Task<int> CreateOrUpdateAsync(Invoice invoice);

        Task<List<Invoice>> GetInvoicesAsync();
    }
}
