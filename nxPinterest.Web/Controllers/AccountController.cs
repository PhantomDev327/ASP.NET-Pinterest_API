using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Certification()
        {
            return View();
        }
    }
}
