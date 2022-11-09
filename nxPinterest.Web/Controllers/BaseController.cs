using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace nxPinterest.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public BaseController() { 
        
        }

        protected string UserId
        {
            get {
                return User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }

        protected string GenerateUrl()
        {
            var uriBuilder = new UriBuilder(Request.Scheme, Request.Host.Host, Request.Host.Port ?? -1);
            if (uriBuilder.Uri.IsDefaultPort) uriBuilder.Port = -1;

            var baseUri = uriBuilder.Uri.AbsoluteUri;

            var fomatUrl = $"{baseUri}shared/${Guid.NewGuid().ToString().Replace("-", "")}";

            return fomatUrl;
        }
    }
}
