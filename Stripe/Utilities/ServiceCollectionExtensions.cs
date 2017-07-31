using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Stripe
{
    public static class ServiceCollectionExtensions
    {
        public static BillingSettings AddBillingSettingsServices(this IServiceCollection services,
            IConfigurationRoot root)
        {
            var billingSettings = new BillingSettings();
            ConfigurationBinder.Bind(root.GetSection("BillingSettings"), billingSettings);
            services.AddSingleton(s => billingSettings);
            return billingSettings;
        }
    }
}
