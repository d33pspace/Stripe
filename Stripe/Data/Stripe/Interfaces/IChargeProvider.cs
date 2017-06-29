namespace Stripe.Data
{
    public interface IChargeProvider
    {
        bool CreateCharge(int amount, string currency, string description, string customerId, out string error);

    }
}