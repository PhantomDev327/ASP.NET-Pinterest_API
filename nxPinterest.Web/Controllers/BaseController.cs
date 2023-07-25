using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using System;
using System.Security.Claims;

namespace nxPinterest.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public const string SessionContainerName = "_Container";
        public BaseController() {
        }

        protected string UserId
        {
            get {
                return User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }

        protected int container_id { 
            get {
                return HttpContext.Session.GetInt32(SessionContainerName) ?? 0;
            }
            set
            { 
                HttpContext.Session.SetInt32(SessionContainerName, value);
            } 
        }

        protected string GeneratePathUrl()
        {
            // random path url
            return $"{Guid.NewGuid().ToString().Replace("-", "")}{DateTime.Now.ToString("yyyyMMddhhmmsss")}";
        }
    }
}
