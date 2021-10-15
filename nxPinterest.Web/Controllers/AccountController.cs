using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nxPinterest.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace nxPinterest.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Certification()
        {
            Services.Models.Request.LoginRequest vm = new Services.Models.Request.LoginRequest();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Certify(Services.Models.Request.LoginRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, true, false);
                    if (result.Succeeded)
                        return RedirectToAction("Index", "Home");
                    else
                    {
                        throw new Exception("Incorrect User name or Password");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["custom-validation-message"] = ex.Message;
            }

            return View("Certification", request);
        }

        public async Task<IActionResult> LogOut()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            Services.Models.Request.RegistrationRequest vm = new Services.Models.Request.RegistrationRequest();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Register(Services.Models.Request.RegistrationRequest vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await this._userManager.CreateAsync(new ApplicationUser()
                    {
                        UserName = vm.Email,
                        Email = vm.Email
                    }, vm.Password);

                    if (result.Succeeded)
                        return RedirectToAction(nameof(Certification));
                    else
                    {
                        throw new Exception(result.Errors.FirstOrDefault().Description);
                    }

                }
            }
            catch (Exception ex)
            {
                ViewData["custom-validation-message"] = ex.Message;
            }

            return View(vm);
        }
    }
}
