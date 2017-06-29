using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Stripe.Data;
using Stripe.Models;

namespace Stripe.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170629141539_EntitiesAdded")]
    partial class EntitiesAdded
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Stripe.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<bool>("Delinquent");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("IPAddress");

                    b.Property<string>("IPAddressCountry");

                    b.Property<DateTime?>("LastLoginTime");

                    b.Property<string>("LastName");

                    b.Property<decimal>("LifetimeValue");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<DateTime?>("RegistrationDate");

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("StripeCustomerId");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Stripe.Models.BillingAddress", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AddressLine1");

                    b.Property<string>("AddressLine2");

                    b.Property<string>("City");

                    b.Property<string>("Country");

                    b.Property<string>("Name");

                    b.Property<string>("State");

                    b.Property<string>("Vat");

                    b.Property<string>("ZipCode");

                    b.HasKey("Id");

                    b.ToTable("BillingAddress");
                });

            modelBuilder.Entity("Stripe.Models.CreditCard", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AddressCity");

                    b.Property<string>("AddressCountry");

                    b.Property<string>("AddressLine1");

                    b.Property<string>("AddressState");

                    b.Property<string>("AddressZip");

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("CardCountry");

                    b.Property<string>("Cvc");

                    b.Property<string>("ExpirationMonth");

                    b.Property<string>("ExpirationYear");

                    b.Property<string>("Fingerprint");

                    b.Property<string>("Last4");

                    b.Property<string>("Name");

                    b.Property<string>("StripeId");

                    b.Property<string>("Type");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("CreditCards");
                });

            modelBuilder.Entity("Stripe.Models.Invoice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AmountDue");

                    b.Property<int?>("ApplicationFee");

                    b.Property<string>("ApplicationUserId");

                    b.Property<int?>("AttemptCount");

                    b.Property<bool?>("Attempted");

                    b.Property<int?>("BillingAddressId");

                    b.Property<bool?>("Closed");

                    b.Property<string>("Currency");

                    b.Property<string>("Customer");

                    b.Property<DateTime?>("Date");

                    b.Property<string>("Description");

                    b.Property<int?>("EndingBalance");

                    b.Property<bool?>("Forgiven");

                    b.Property<DateTime?>("NextPaymentAttempt");

                    b.Property<bool?>("Paid");

                    b.Property<DateTime?>("PeriodEnd");

                    b.Property<DateTime?>("PeriodStart");

                    b.Property<string>("ReceiptNumber");

                    b.Property<int?>("StartingBalance");

                    b.Property<string>("StatementDescriptor");

                    b.Property<string>("StripeCustomerId")
                        .HasMaxLength(50);

                    b.Property<string>("StripeId")
                        .HasMaxLength(50);

                    b.Property<int?>("Subtotal");

                    b.Property<int?>("Tax");

                    b.Property<decimal?>("TaxPercent");

                    b.Property<int?>("Total");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.HasIndex("BillingAddressId");

                    b.ToTable("Invoices");
                });

            modelBuilder.Entity("Stripe.Models.Invoice+LineItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Amount");

                    b.Property<string>("Currency");

                    b.Property<int?>("InvoiceId");

                    b.Property<int?>("PeriodId");

                    b.Property<int?>("PlanId");

                    b.Property<bool>("Proration");

                    b.Property<int?>("Quantity");

                    b.Property<string>("StripeLineItemId");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.HasIndex("PeriodId");

                    b.HasIndex("PlanId");

                    b.ToTable("LineItem");
                });

            modelBuilder.Entity("Stripe.Models.Invoice+Period", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("End");

                    b.Property<DateTime?>("Start");

                    b.HasKey("Id");

                    b.ToTable("Period");
                });

            modelBuilder.Entity("Stripe.Models.Invoice+Plan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AmountInCents");

                    b.Property<DateTime?>("Created");

                    b.Property<string>("Currency");

                    b.Property<string>("Interval");

                    b.Property<int>("IntervalCount");

                    b.Property<string>("Name");

                    b.Property<string>("StatementDescriptor");

                    b.Property<int?>("TrialPeriodDays");

                    b.HasKey("Id");

                    b.ToTable("Plan");
                });

            modelBuilder.Entity("Stripe.Models.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("End");

                    b.Property<string>("ReasonToCancel");

                    b.Property<DateTime?>("Start");

                    b.Property<string>("Status");

                    b.Property<string>("StripeId")
                        .HasMaxLength(50);

                    b.Property<string>("SubscriptionPlanId");

                    b.Property<decimal>("TaxPercent");

                    b.Property<DateTime?>("TrialEnd");

                    b.Property<DateTime?>("TrialStart");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("SubscriptionPlanId");

                    b.HasIndex("UserId");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("Stripe.Models.SubscriptionPlan", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Currency");

                    b.Property<bool>("Disabled");

                    b.Property<int>("Interval");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<double>("Price");

                    b.Property<int>("TrialPeriodInDays");

                    b.HasKey("Id");

                    b.ToTable("SubscriptionPlans");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Stripe.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Stripe.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Stripe.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Stripe.Models.CreditCard", b =>
                {
                    b.HasOne("Stripe.Models.ApplicationUser")
                        .WithMany("CreditCards")
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Stripe.Models.Invoice", b =>
                {
                    b.HasOne("Stripe.Models.ApplicationUser")
                        .WithMany("Invoices")
                        .HasForeignKey("ApplicationUserId");

                    b.HasOne("Stripe.Models.BillingAddress", "BillingAddress")
                        .WithMany()
                        .HasForeignKey("BillingAddressId");
                });

            modelBuilder.Entity("Stripe.Models.Invoice+LineItem", b =>
                {
                    b.HasOne("Stripe.Models.Invoice")
                        .WithMany("LineItems")
                        .HasForeignKey("InvoiceId");

                    b.HasOne("Stripe.Models.Invoice+Period", "Period")
                        .WithMany()
                        .HasForeignKey("PeriodId");

                    b.HasOne("Stripe.Models.Invoice+Plan", "Plan")
                        .WithMany()
                        .HasForeignKey("PlanId");
                });

            modelBuilder.Entity("Stripe.Models.Subscription", b =>
                {
                    b.HasOne("Stripe.Models.SubscriptionPlan", "SubscriptionPlan")
                        .WithMany()
                        .HasForeignKey("SubscriptionPlanId");

                    b.HasOne("Stripe.Models.ApplicationUser", "User")
                        .WithMany("Subscriptions")
                        .HasForeignKey("UserId");
                });
        }
    }
}
