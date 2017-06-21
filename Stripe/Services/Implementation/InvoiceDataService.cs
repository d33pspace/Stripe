using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stripe.Data;
using Stripe.Models;
using Stripe.Services;

namespace Stripe.Services
{
    public class InvoiceDataService<TContext, TUser> : IInvoiceDataService
        where TContext : ApplicationDbContext
        where TUser : ApplicationUser
    {
        private readonly TContext _dbContext;

        public InvoiceDataService(TContext context)
        {
            this._dbContext = context;
        }

        public async Task<List<Invoice>> UserInvoicesAsync(string userId)
        {
            return await _dbContext.Invoices.Where(i => i.Customer == userId).Select(s => s).ToListAsync();
        }

        public async Task<Invoice> UserInvoiceAsync(string userId, int invoiceId)
        {
            return await _dbContext.Invoices.Where(i => i.Customer == userId && i.Id == invoiceId).Select(s => s).FirstOrDefaultAsync();
        }

        public async Task<int> CreateOrUpdateAsync(Invoice invoice)
        {
            var res = -1;

            var dbInvoice = _dbContext.Invoices.Find(invoice.Id);

            if (dbInvoice == null)
            {
                var user = await _dbContext.Users.Where(u => u.StripeCustomerId == invoice.StripeCustomerId).FirstOrDefaultAsync();

                if (user != null)
                {
                    invoice.Customer = user.Id;
                    _dbContext.Invoices.Add(invoice);
                    res = await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                dbInvoice.Paid = invoice.Paid;
                dbInvoice.Attempted = invoice.Attempted;
                dbInvoice.AttemptCount = invoice.AttemptCount;
                dbInvoice.NextPaymentAttempt = invoice.NextPaymentAttempt;
                dbInvoice.Closed = invoice.Closed;
                res = await _dbContext.SaveChangesAsync();
            }

            return res;
        }

        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            var invoices = await _dbContext.Invoices.Include(i => i.Customer).Select(i => i).ToListAsync();

            return invoices;
        }
    }
}
