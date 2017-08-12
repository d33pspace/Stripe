using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Stripe.Controllers
{
    public class BaseController : Controller
    {
        public const string TempMessage = "$tempMessage";

        public string GetTempMessage()
        {
            var tempMessage = HttpContext.Session.GetString("tempMessage");
            HttpContext.Session.Remove(TempMessage);
            return tempMessage;
        }

        public void SetTempMessage(string message)
        {
            HttpContext.Session.SetString(TempMessage, message);
        }
    }
}