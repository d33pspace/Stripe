using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stripe.Data;
using Stripe.Models;
using Stripe;
using Stripe.Services;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Mvc;

namespace Stripe
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Options
            services.AddOptions();

            // Billing Settings
            var globalSettings = services.AddBillingSettingsServices(Configuration);
            services.Configure<BillingSettings>(Configuration.GetSection("BillingSettings"));

            StripeConfiguration.SetApiKey(globalSettings.StripeApiKey);

            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc(options =>
            {
                //options.Filters.Add(new RequireHttpsAttribute());
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetService<ApplicationDbContext>();

            services.AddSingleton<ICardDataService>(new CardDataService<ApplicationDbContext, ApplicationUser>(dbContext));
            services.AddSingleton<IInvoiceDataService>(new InvoiceDataService<ApplicationDbContext, ApplicationUser>(dbContext));
            services.AddSingleton<ISubscriptionDataService>(new SubscriptionDataService<ApplicationDbContext, ApplicationUser>(dbContext));
            services.AddSingleton<ISubscriptionPlanDataService>(new SubscriptionPlanDataService<ApplicationDbContext, ApplicationUser>(dbContext));

            // Stripe services
            services.AddTransient<ICardProvider, CardProvider>();
            services.AddTransient<IChargeProvider, ChargeProvider>();
            services.AddTransient<ICustomerProvider, CustomerProvider>();
            services.AddTransient<ISubscriptionPlanProvider, SubscriptionPlanProvider>();
            services.AddTransient<ISubscriptionProvider, SubscriptionProvider>();

            services.AddScoped<SubscriptionsFacade>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationDbContext appDbContext)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            var options = new RewriteOptions()
                .AddRedirectToHttps();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            appDbContext.Database.Migrate();
        }
    }
}
