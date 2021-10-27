using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nxPinterest.Services.Models;
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

        [Route("Account/Forgot-Password")]
        public IActionResult ForgotPassword() {
            Services.Models.Request.ForgotPasswordRequest vm = new Services.Models.Request.ForgotPasswordRequest();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(Services.Models.Request.ForgotPasswordRequest request) {
            try
            {
                bool isValid = ModelState.IsValid;

                if (isValid)
                {
                    var user = await this._userManager.FindByEmailAsync(request.Email);
                    if (user == null) throw new Exception("User email not found!");

                    var result = await this._userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                    if (result.Succeeded)
                        TempData["custom-validation-success-message"] = "Password has been successfully changed!";
                    else
                        throw new Exception(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }
              
                return Redirect(nameof(Certification));
            }
            catch (Exception ex)
            {
                TempData["custom-validation-message"] = ex.Message;
                return Redirect("/account/forgot-password");
            }
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

                    if (result.Succeeded) {
                        TempData["custom-validation-success-message"] = "User has been successfully registered!";
                        return RedirectToAction(nameof(Certification));
                    }
                    else
                    {
                        throw new Exception(result.Errors.FirstOrDefault().Description);
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["custom-validation-message"] = ex.Message;
            }

            return View(vm);
        }
    }
}
