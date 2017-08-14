using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stripe
{
    public class StripeSettings
    {
        public virtual string SecretKey { get; set; }

        public virtual string PublishableKey { get; set; }

        public virtual string StatementDescriptor { get; set; }
    }
}
