using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace KickFive.Controllers
{
    public class LanguageController : Controller
    {
        public IActionResult Index(string culture, string? returnUrl = null)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            // Alleen het pad + query gebruiken, nooit de volledige Referer-URL
            var localUrl = "/";
            var referer = Request.Headers["Referer"].ToString();
            if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            {
                localUrl = refererUri.PathAndQuery;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                localUrl = returnUrl;
            }

            return LocalRedirect(localUrl);
        }
    }
}

