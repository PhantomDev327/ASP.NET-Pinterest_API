using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nxPinterest.Services.Models;
using nxPinterest.Data.Models;
using nxPinterest.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using nxPinterest.Data;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace nxPinterest.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Services.Interfaces.IUserContainerManagementService userContainerManagementService;
        public const int pageSize = nxPinterest.Services.dev_Settings.pageSize_regist;
        private readonly ApplicationDbContext _context;
        protected string UserId
        {
            get
            {
                return User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }
        public AccountController(UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                Services.Interfaces.IUserContainerManagementService userContainerManagementService,
                                ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            this.userContainerManagementService = userContainerManagementService;
        }

        // START ------------------------ 共通処理 ------------------------

        public IActionResult Certification()
        {
            Services.Models.Request.LoginRequest vm = new Services.Models.Request.LoginRequest();
            return View(vm);
        }

        // ログイン認証処理
        [HttpPost]
        public async Task<IActionResult> Certify(Services.Models.Request.LoginRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, true, false);
                    if (result.Succeeded)
                    {
                        var userinfo = await _userManager.FindByEmailAsync(request.Email);

                        Data.Models.UserContainer container = await (this._context.UserContainer.AsNoTracking()
                                                         .FirstOrDefaultAsync(c => c.container_id.Equals(userinfo.container_id)));

                        if (userinfo.user_visibility == false || (container != null && container.container_visibility == false))
                        {
                            throw new Exception("This User is Invalid");
                        }
                        if (userinfo.Discriminator == "SysAdmin")
                        {
                            return RedirectToAction("UserContainerIdList", "account");
                        }else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    throw new Exception("Incorrect User name or Password");
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return View("Certification", request);
        }

        // ログアウト
        public async Task<IActionResult> LogOut()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        //　ユーザ登録
        public IActionResult Register()
        {
            Services.Models.Request.RegistrationRequest vm = new Services.Models.Request.RegistrationRequest();
            return View(vm);
        }

        // 画像登録
        public IActionResult ImageRegister()
        {
            Models.ImageRegisterModel vm = new Models.ImageRegisterModel();
            return View(vm);
        }

        // パスワード忘れ
        [Route("Account/Forgot-Password")]
        public IActionResult ForgotPassword() {
            Services.Models.Request.ForgotPasswordRequest vm = new Services.Models.Request.ForgotPasswordRequest();
            return View(vm);
        }

        //パスワード設定
        public IActionResult SetPassword()
        {
            var value = RouteData.Values["id"].ToString();
            var encryptionKey = "770A8A65DA156D24";
            Services.Models.Request.SetPasswordRequest vm = new Services.Models.Request.SetPasswordRequest();
            if (value != null)
            {
                var activationCode = DecryptRijndael(value, encryptionKey);
                if (IsEmailExistsAsync(activationCode))
                {
                    ViewBag.Message = "Activation successful.";
                }
            }
            return View(vm);
        }

        // パスワードセット
        [HttpPost]
        public async Task<IActionResult> SetPassWord(Services.Models.Request.SetPasswordRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var value = RouteData.Values["id"].ToString();
                    var encryptionKey = "770A8A65DA156D24";
                    if (value != null)
                    {
                        var activationMail= DecryptRijndael(value, encryptionKey);
                        var user = await this._userManager.FindByEmailAsync(activationMail);

                        var result = await this._userManager.AddPasswordAsync(user, request.NewPassword);
                        if (result.Succeeded)
                        {
                            TempData["custom-validation-success-message"] = "Password has been successfully Registered!";

                        }
                        else
                            throw new Exception(result.Errors.FirstOrDefault().Description);
                    }

                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return Redirect("/Account/Certification");
        }

        // パスワードリセットの確認
        [HttpPost]
        public async Task<IActionResult> ResetPasswordConfirm(Services.Models.Request.SetPasswordRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    var value = request.Email;
                    var encryptionKey = "770A8A65DA156D24";
                    if (value != null)
                    {
                        var activationMail = DecryptRijndael(value, encryptionKey);
                        var user = await this._userManager.FindByEmailAsync(activationMail);

                        var removePw = await this._userManager.RemovePasswordAsync(user);

                        if (removePw.Succeeded)
                        {
                            var result = await this._userManager.AddPasswordAsync(user, request.NewPassword);

                            if (result.Succeeded)
                            {
                                TempData["custom-validation-success-message"] = "Password has been successfully Changed!";
                            }
                            else
                                throw new Exception(result.Errors.FirstOrDefault().Description);
                         }   
                    }

                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return Redirect("/Account/Certification");
        }

        //パスワード再設定
        public IActionResult ResetPassword()
        {
            var value = RouteData.Values["id"].ToString();
            var encryptionKey = "770A8A65DA156D24";
            Services.Models.Request.SetPasswordRequest vm = new Services.Models.Request.SetPasswordRequest();
            if (value != null)
            {
                var activationCode = DecryptRijndael(value, encryptionKey);
                if (IsEmailExistsAsync(activationCode))
                {
                    ViewBag.Message = "Activation successful.";
                }

                vm.Email = value;
            }
            return View(vm);
        }

        // パスワードリセット
        [HttpPost]
        public async Task<IActionResult> ResetPassword(Services.Models.Request.ForgotPasswordRequest request) {
            try
            {
                bool isValid = ModelState.IsValid;

                if (isValid)
                {
                    var user = await this._userManager.FindByEmailAsync(request.Email);
                    if (user == null) throw new Exception("User email not found!");

                    Data.Models.UserContainer container = await (this._context.UserContainer.AsNoTracking()
                                                     .FirstOrDefaultAsync(c => c.container_id.Equals(user.container_id)));

                    if (user.user_visibility == false || container.container_visibility == false)
                    {
                        throw new Exception("This User is Invalid");
                    }

                    // メール送信

                    var encryptionKey = "770A8A65DA156D24";
                    var Email = user.ToString();
                    var value = EncryptRijndael(Email, encryptionKey);
                    var activationCode = value.Replace('/', '-').Replace('+', '_').PadRight(4 * ((value.Length + 3) / 4), '=');

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("starse.noreply@gmail.com");
                        mail.To.Add(Email);
                        mail.Subject = "パスワードリセットのお知らせ";
                        mail.Body = "パスワードリセットの申請を受け付けました。<br />パスワードの再設定をご希望の場合は、以下URLをクリックし新しいパスワードをご登録ください。<br /><a href = '" + string.Format("{0}://{1}/Account/ResetPassword/{2}", Request.Scheme, Request.Host, activationCode) + "'>Click here to reset your password.</a>";
                        mail.IsBodyHtml = true;

                        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.Credentials = new NetworkCredential("starse.noreply@gmail.com", "starse2022");
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }

                    TempData["custom-validation-success-message"] = "Password reset mail has been sent。";
                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }

                return Redirect("/account/forgot-password");
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
        }

        // パスワード変更
        [Route("Account/Change-Password")]
        public IActionResult ChangePassword()
        {
            Services.Models.Request.ChangePasswordRequest vm = new Services.Models.Request.ChangePasswordRequest();

            var user = this._userManager.FindByIdAsync(this.UserId);
            ViewBag.UserRole = user.Result.Discriminator;
            return View(vm);
        }

        // パスワード変更の確認
        [HttpPost]
        public async Task<IActionResult>ChangePasswordConfirm(Services.Models.Request.ChangePasswordRequest request)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await this._userManager.FindByIdAsync(this.UserId);
                    var result = await this._userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                    if (result.Succeeded)
                    {
                        ViewBag.UserRole = user.Discriminator;
                        TempData["custom-validation-success-message"] = "Password has been successfully changed!";
                    }
                    else
                        throw new Exception(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }
                return Redirect("/Account/Change-Password");
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
        }

        // ユーザ登録後のパスワード設定
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
                        var user = await _userManager.FindByEmailAsync(vm.Email);
                        if (user != null)
                        {
                            user.Discriminator = (_context.Users.ToList().Count == 1) ? "Admin" : "ApplicationUser";
                        }
                        var update = await this._userManager.UpdateAsync(user);

                        if (update.Succeeded)
                        {
                            // メール送信
                            var encryptionKey = "770A8A65DA156D24";
                            var Email = user.ToString();
                            var value = EncryptRijndael(Email, encryptionKey);
                            var activationCode = value.Replace('/', '-').Replace('+', '_').PadRight(4 * ((value.Length + 3) / 4), '=');
                            using (MailMessage mail = new MailMessage())
                            {
                                mail.From = new MailAddress("starse.noreply@gmail.com");
                                mail.To.Add(Email);
                                mail.Subject = "登録完了のお知らせ";
                                mail.Body = "登録が完了いたしましたので、ご連絡いたします。<br />以下URLをクリックしパスワードをご登録ください。< br /><a href = '" + string.Format("{0}://{1}/Account/SetPassword/{2}", Request.Scheme, Request.Host, activationCode) + "'>Click here to activate your account.</a>";
                                mail.IsBodyHtml = true;

                                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                                {
                                    smtp.Credentials = new NetworkCredential("starse.noreply@gmail.com", "starse2022");
                                    smtp.EnableSsl = true;
                                    smtp.Send(mail);
                                }
                            }

                            TempData["custom-validation-success-message"] = "User has been successfully registered!";
                            return RedirectToAction(nameof(Certification));

                        }
                    }
                    throw new Exception(result.Errors.FirstOrDefault().Description);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return View(vm);
        }

        /*
         * メールのEncrypt
         */
        public static string EncryptRijndael(string value, string encryptionKey)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(encryptionKey); //must be 16 chars
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                var rijndael = new RijndaelManaged
                {
                    BlockSize = 128,
                    IV = key,
                    KeySize = 128,
                    Key = key
                };
#pragma warning restore SYSLIB0022 // Type or member is obsolete

                var transform = rijndael.CreateEncryptor();
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(value);

                        cs.Write(buffer, 0, buffer.Length);
                        cs.FlushFinalBlock();
                        cs.Close();
                    }
                    ms.Close();
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /*
         * メールのDecrypt
         */
        public static string DecryptRijndael(string value, string encryptionKey)
        {
            try
            {
                var key = Encoding.UTF8.GetBytes(encryptionKey); //must be 16 chars
#pragma warning disable SYSLIB0022 // Type or member is obsolete
                var rijndael = new RijndaelManaged
                {
                    BlockSize = 128,
                    IV = key,
                    KeySize = 128,
                    Key = key
                };
#pragma warning restore SYSLIB0022 // Type or member is obsolete

                value = value.Replace('-', '/').Replace('_', '+').PadRight(4 * ((value.Length + 3) / 4), '=');
                var buffer = Convert.FromBase64String(value);
                var transform = rijndael.CreateDecryptor();
                string decrypted;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                    {
                        cs.Write(buffer, 0, buffer.Length);
                        cs.FlushFinalBlock();
                        decrypted = Encoding.UTF8.GetString(ms.ToArray());
                        cs.Close();
                    }
                    ms.Close();
                }

                return decrypted;
            }
            catch
            {
                return null;
            }
        }

        /*
         * メール存在チェック
         */
        public bool IsEmailExistsAsync(string eMail)
        {
            var user = _userManager.FindByEmailAsync(eMail);
            if (user == null)
            {
                return false;
            }

            return true;
        }

        // END ------------------------ 共通処理 ------------------------

        // START ------------------------ システム管理者 ＞コンテナユーザ一覧/登録/変更/削除------------------------

        /*
         * コンテナユーザ情報の一覧表示
         */
        /// <summary>
        /// Get view ContainerUserList
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<IActionResult> UserContainerIdList(int pageIndex = 1)
        {
            UserListViewModel vm = new UserListViewModel();
            try
            {
                // システム管理者のみ利用可能な機能
                var user = await this._userManager.FindByIdAsync(this.UserId);
                if (user.Discriminator != "SysAdmin")
                {
                    throw new Exception("Access Denied");
                }

                vm.ApplicationUserList = _context.Users.Where(c => c.Discriminator.Equals("ContainerAdmin")).ToList();

                Task<IList<Data.Models.UserContainer>> userContainerlist = this.userContainerManagementService.ListContainerAsyc();
                Dictionary<int, string> dicUserContainer = new Dictionary<int, string>();

                if (userContainerlist.Result != null)
                {
                    foreach (var item in userContainerlist.Result)
                    {
                        if (item.container_visibility == true)
                        {
                            dicUserContainer.Add(item.container_id, item.container_name);
                        }
                    }
                }
                ViewBag.ContainerIdList = dicUserContainer;
                int totalPages = (int)System.Math.Ceiling((decimal)(vm.ApplicationUserList.Count / (decimal)pageSize));
                int skip = (pageIndex - 1) * pageSize;
                int totalRecordCount = vm.ApplicationUserList.Count;

                ViewBag.ItemCount = vm.ApplicationUserList.Count;

                vm.ApplicationUserList = vm.ApplicationUserList.Skip(skip).Take(pageSize).ToList();

                vm.PageIndex = pageIndex;
                vm.TotalPages = totalPages;
                vm.TotalRecords = totalRecordCount;
                vm.CurrentPageName = "userlist";

                return this.View("~/Views/Account/UserContainerIdList.cshtml", vm);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return this.View("~/Views/Account/UserContainerIdList.cshtml", vm);
        }

        /*
         * コンテナユーザ情報の新規登録　初期画面
         */
        public async Task<IActionResult> UserContainerIdRegister()
        {
            // システム管理者のみ利用可能な機能
            var user = await this._userManager.FindByIdAsync(this.UserId);
            if (user.Discriminator != "SysAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }

            Services.Models.Request.UserRegistrationRequest vm = new Services.Models.Request.UserRegistrationRequest();
  
            Task<IList<Data.Models.UserContainer>> list = this.userContainerManagementService.ListContainerAsyc();
            ViewBag.ContainerList = (list.Result != null) ? list.Result.Where(c => c.container_visibility.Equals(true)) : new List<Data.Models.UserContainer>();
            return this.View("~/Views/Account/UserContainerIdRegister.cshtml", vm);
        }

        /*
         * コンテナユーザ情報の新規登録　処理
         */
        [HttpPost]
        public async Task<IActionResult> UserContainerIdRegister(Services.Models.Request.UserRegistrationRequest vm)
        {
            try
            {
                // システム管理者のみ利用可能な機能
                var loginUser = await this._userManager.FindByIdAsync(this.UserId);
                if (loginUser.Discriminator != "SysAdmin")
                {
                    throw new Exception("Access Denied");
                }

                if (ModelState.IsValid)
                {
                    var result = await this._userManager.CreateAsync(new ApplicationUser()
                    {
                        UserName = vm.Email,
                        container_id = vm.container_id,
                        user_visibility = true,
                        PhoneNumber = vm.PhoneNumber,
                        Discriminator = "ContainerAdmin",
                        Email = vm.Email,
                        UserDispName = vm.UserDispName
                    });


                    if (result.Succeeded)
                    {

                        var user = await _userManager.FindByEmailAsync(vm.Email);
                        
                        var encryptionKey = "770A8A65DA156D24";
                        var Email = user.ToString();
                        var value = EncryptRijndael(Email, encryptionKey);
                        var activationCode = value.Replace('/', '-').Replace('+', '_').PadRight(4 * ((value.Length + 3) / 4), '=');
                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("starse.noreply@gmail.com");
                            mail.To.Add(Email);
                            mail.Subject = "ユーザー登録のお知らせ";
                            mail.Body = "ユーザー登録の申請を受け付けました。<br />パスワードの設定をご希望の場合は、以下URLをクリックし新しいパスワードをご登録ください。<br /><a href = '" + string.Format("{0}://{1}/Account/SetPassword/{2}", Request.Scheme, Request.Host, activationCode) + "'>Click here to set your password.</a>";
                            mail.IsBodyHtml = true;

                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                smtp.Credentials = new NetworkCredential("starse.noreply@gmail.com", "starse2022");
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }

                        TempData["custom-validation-success-message"] = "User has been successfully registered!";
                        return RedirectToAction(nameof(UserContainerIdList));
                    }
                    throw new Exception(result.Errors.FirstOrDefault().Description);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            Task<IList<Data.Models.UserContainer>> list = this.userContainerManagementService.ListContainerAsyc();
            ViewBag.ContainerList = (list.Result != null) ? list.Result.Where(c => c.container_visibility.Equals(true)) : new List<Data.Models.UserContainer>();
            return this.View("~/Views/Account/UserContainerIdRegister.cshtml");
        }

        /*
         * コンテナユーザ情報の変更　初期画面
         */
        /// <summary>
        /// Get view NormalUserEdit
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IActionResult> UserContainerIdEdit(String email)
        {
            // システム管理者のみ利用可能な機能
            var user = await this._userManager.FindByIdAsync(this.UserId);
            if (user.Discriminator != "SysAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }

            UserEditViewModel vm = new UserEditViewModel();
            String userEmail = email.Trim();
            IList<Data.Models.ApplicationUser> list = this._context.Users.Where(c => c.Email.Equals(userEmail)).ToList();

            foreach (var usr in list)
            {
                vm.Email = userEmail;
                vm.UserName = userEmail;
                vm.container_id = usr.container_id;
                vm.PhoneNumber = usr.PhoneNumber;
                vm.user_visibility = usr.user_visibility;
                vm.UserDispName = usr.UserDispName;
            }
            Task<IList<Data.Models.UserContainer>> containerlist = this.userContainerManagementService.ListContainerAsyc();
            ViewBag.ContainerList = (containerlist != null) ? containerlist.Result.Where(c => c.container_visibility.Equals(true)) : new List<Data.Models.UserContainer>();

            return this.View("~/Views/Account/UserContainerIdEdit.cshtml", vm);
        }

        /*
         * コンテナユーザ情報の変更　処理
         */
        [HttpPost]
        public async Task<IActionResult> UserContainerIdEdit(Web.Models.UserEditViewModel vm)
        {
            try
            {
                // システム管理者のみ利用可能な機能
                var loginUser = await this._userManager.FindByIdAsync(this.UserId);
                if (loginUser.Discriminator != "SysAdmin")
                {
                    throw new Exception("Access Denied");
                }

                if (ModelState.IsValid)
                {
                    vm.Email = vm.Email.Trim();
                    var user = await _userManager.FindByEmailAsync(vm.Email);
                    if (user != null)
                    {
                        user.Email = vm.Email;
                        user.UserName = vm.Email;
                        user.PhoneNumber = vm.PhoneNumber;
                        user.user_visibility = vm.user_visibility;
                        user.UserDispName = vm.UserDispName;
                    }
                    var update = await this._userManager.UpdateAsync(user);
                    if (update.Succeeded)
                    {
                        TempData["custom-validation-success-message"] = "User Infromation has been successfully changed!";
                        return RedirectToAction(nameof(UserContainerIdList));
                    }
                    throw new Exception(update.Errors.FirstOrDefault().Description);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return this.View("~/Views/Account/UserContainerIdEdit.cshtml", vm);
        }

        // END ------------------------ システム管理者＞コンテナユーザ一覧/登録/変更/削除 --------------------

        // START ------------------------ システム管理者＞コンテナ一覧/登録/変更/削除 ------------------------

        /*
         * コンテナ情報の一覧表示
         */
        /// <summary>
        /// Get view Container User List
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<IActionResult> UserContainerList(int pageIndex = 1)
        {
            // システム管理者のみ利用可能な機能
            var user = await this._userManager.FindByIdAsync(this.UserId);
            if (user.Discriminator != "SysAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }

            ContainerListViewModel vm = new ContainerListViewModel();

            Task<IList<Data.Models.UserContainer>> list = userContainerManagementService.ListContainerAsyc();
            vm.containerList = (list.Result == null) ? new List<Data.Models.UserContainer>() : list.Result;

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.containerList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;
            int totalRecordCount = vm.containerList.Count;

            ViewBag.ItemCount = vm.containerList.Count;

            vm.containerList = vm.containerList.Skip(skip).Take(pageSize).ToList();

            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;
            vm.TotalRecords = totalRecordCount;
            vm.CurrentPageName = "containerList";

            return this.View("~/Views/Account/UserContainerList.cshtml", vm);
        }

        /*
         * コンテナ情報の新規登録　初期画面
         */
        public IActionResult UserContainerRegister()
        {
            // システム管理者のみ利用可能な機能
            var user = this._userManager.FindByIdAsync(this.UserId);
            if (user.Result.Discriminator != "SysAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }
            Services.Models.Request.UserContainerRegistrationRequest vm = new Services.Models.Request.UserContainerRegistrationRequest();
            return this.View("~/Views/Account/UserContainerRegister.cshtml", vm);
        }

        /*
         * コンテナ情報の新規登録　処理
         */
        [HttpPost]
        public async Task<IActionResult> UserContainerRegister(Services.Models.Request.UserContainerRegistrationRequest vm)
        {
            try
            {
                // システム管理者のみ利用可能な機能
                var user = await this._userManager.FindByIdAsync(this.UserId);
                if (user.Discriminator != "SysAdmin")
                {
                    TempData["Message"] = "Access Denied!";
                    return View("~/Views/Error/204.cshtml");
                }

                if (ModelState.IsValid)
                {
                    UserContainer container = new UserContainer();
                    container.container_name = vm.container_name;
                    container.container_visibility = true;
                    this.userContainerManagementService.InsertUserContainer(container);
                    TempData["custom-validation-success-message"] = "Container has been successfully registered!";
                    return RedirectToAction(nameof(UserContainerList));
                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                TempData["custom-validation-message"] = ex.Message;
            }
            return this.View("~/Views/Account/UserContainerRegister.cshtml", vm);
        }

        /*
         * コンテナ情報の変更　初期画面
         */
        public IActionResult UserContainerEdit(int container_id)
        {
            // システム管理者のみ利用可能な機能
            var user = this._userManager.FindByIdAsync(this.UserId);
            if (user.Result.Discriminator != "SysAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }

            Services.Models.Request.UserContainerRegistrationRequest vm = new Services.Models.Request.UserContainerRegistrationRequest();
            IList<Data.Models.UserContainer> list = this._context.UserContainer.Where(c => c.container_id.Equals(container_id)).ToList();
            foreach (var usr in list)
            {
                vm.container_id = usr.container_id;
                vm.container_name = usr.container_name;
                vm.container_visibility = usr.container_visibility;
            }
            return this.View("~/Views/Account/UserContainerEdit.cshtml", vm);
        }

        /*
         * コンテナ情報の変更　処理
         */
        [HttpPost]
        public async Task<IActionResult> UserContainerEdit(Services.Models.Request.UserContainerRegistrationRequest vm)
        {
            try
            {
                // システム管理者のみ利用可能な機能
                var loginuser = await this._userManager.FindByIdAsync(this.UserId);
                if (loginuser.Discriminator != "SysAdmin")
                {
                    TempData["Message"] = "Access Denied!";
                    return View("~/Views/Error/204.cshtml");
                }

                if (ModelState.IsValid)
                {
                    UserContainer container = new UserContainer();
                    var user = this._context.UserContainer.Where(c => c.container_id.Equals(vm.container_id));
                    if (user != null)
                    {
                        container.container_id = vm.container_id;
                        container.container_name = vm.container_name;
                        container.container_visibility = vm.container_visibility;
                        this.userContainerManagementService.UpdateUserContainer(container);
                        TempData["custom-validation-success-message"] = "Container has been successfully changed!";
                        return RedirectToAction(nameof(UserContainerList));
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }
            return this.View("~/Views/Account/UserContainerEdit.cshtml", vm);
        }


        // END ------------------------ システム管理者＞コンテナ一覧/登録/変更/削除 ------------------------

        // START ------------------------ コンテナ管理者＞ユーザ一覧/登録/変更/削除 ------------------------

        // ユーザ情報の一覧表示
        /// <summary>
        /// Get view NormalUserList
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<IActionResult> NormalUserList(int pageIndex = 1)
        {
            // コンテナ管理者のみ利用可能な機能
            var user = await this._userManager.FindByIdAsync(this.UserId);
            if (user.Discriminator != "ContainerAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }

            UserListViewModel vm = new UserListViewModel();
            UserListViewModel vm2 = new UserListViewModel();

            ApplicationUser u = new ApplicationUser();
            vm2.ApplicationUserList = this._context.Users.Where(c => c.Id.Equals(this.UserId)).ToList();

            vm.ApplicationUserList = (this._context.Users
                                    .Where(c => c.container_id.Equals(vm2.ApplicationUserList.First().container_id))).ToList();

            int totalPages = (int)System.Math.Ceiling((decimal)(vm.ApplicationUserList.Count / (decimal)pageSize));
            int skip = (pageIndex - 1) * pageSize;
            int totalRecordCount = vm.ApplicationUserList.Count;

            ViewBag.ItemCount = vm.ApplicationUserList.Count;

            vm.ApplicationUserList = vm.ApplicationUserList.Skip(skip).Take(pageSize).ToList();

            vm.PageIndex = pageIndex;
            vm.TotalPages = totalPages;
            vm.TotalRecords = totalRecordCount;

            return this.View("~/Views/Account/NormalUserList.cshtml", vm);
        }

        /*
         * ユーザ情報の新規登録　初期画面
         */
        public async Task<IActionResult> NormalUserRegister(int container_id)
        {
            // コンテナ管理者のみ利用可能な機能
            var user = await this._userManager.FindByIdAsync(this.UserId);
            if (user.Discriminator != "ContainerAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }
            string container_id1 = container_id.ToString();
            string container_id2 = container_id1.Trim();
            int container_id3 = Int32.Parse(container_id2);
            Services.Models.Request.NormalUserRegistrationRequest vm = new Services.Models.Request.NormalUserRegistrationRequest();
            vm.container_id = container_id3;
            return this.View("~/Views/Account/NormalUserRegister.cshtml", vm);
        }

        /*
         * ユーザ情報の新規登録　処理
         */
        [HttpPost]
        public async Task<IActionResult> NormalUserRegister(Services.Models.Request.NormalUserRegistrationRequest vm)
        {
            try
            {
                // コンテナ管理者のみ利用可能な機能
                var loginUser = await this._userManager.FindByIdAsync(this.UserId);
                if (loginUser.Discriminator != "ContainerAdmin")
                {
                    throw new Exception("Access Denied");
                }

                if (ModelState.IsValid)
                {
                    ApplicationUser u = await this._userManager.FindByEmailAsync(vm.Email);
                    if(u != null)
                    {
                        TempData["custom-validation-message"] = "User Already Exit";
                        return this.View("~/Views/Account/NormalUserRegister.cshtml", vm);
                    }
                    var result = await this._userManager.CreateAsync(new ApplicationUser()
                    {
                        UserName = vm.Email,
                        UserDispName = vm.UserDispName,
                        user_visibility = true,
                        container_id = vm.container_id,
                        PhoneNumber = vm.PhoneNumber,
                        Discriminator = vm.Discriminator == "一般" ? "ApplicationUser" : "ContainerAdmin",
                        Email = vm.Email
                    });

                    if (result.Succeeded)
                    {

                        var user = await _userManager.FindByEmailAsync(vm.Email);

                        var encryptionKey = "770A8A65DA156D24";
                        var Email = user.ToString();
                        var value = EncryptRijndael(Email, encryptionKey);
                        var activationCode = value.Replace('/', '-').Replace('+', '_').PadRight(4 * ((value.Length + 3) / 4), '=');
                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("starse.noreply@gmail.com");
                            mail.To.Add(Email);
                            mail.Subject = "ユーザー登録のお知らせ";
                            mail.Body = "ユーザー登録の申請を受け付けました。<br />パスワードの設定をご希望の場合は、以下URLをクリックし新しいパスワードをご登録ください。<br /><a href = '" + string.Format("{0}://{1}/Account/SetPassword/{2}", Request.Scheme, Request.Host, activationCode) + "'>Click here to set your password.</a>";
                            mail.IsBodyHtml = true;

                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                smtp.Credentials = new NetworkCredential("starse.noreply@gmail.com", "starse2022");
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }

                        TempData["custom-validation-success-message"] = "User has been successfully registered!";
                        return Redirect("/Account/NormalUserList");
                    }
                    throw new Exception(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                }
                return Redirect("/Account/NormalUserRegister");
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return Redirect("/Account/NormalUserList");
        }

        /*
         * ユーザ情報の変更　初期画面
         */
        /// <summary>
        /// Get view NormalUserEdit
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IActionResult> NormalUserEdit(String email)
        {
            // コンテナ管理者のみ利用可能な機能
            var user = await this._userManager.FindByIdAsync(this.UserId);
            if (user.Discriminator != "ContainerAdmin")
            {
                TempData["Message"] = "Access Denied!";
                return View("~/Views/Error/204.cshtml");
            }

            Services.Models.Request.NormalUserRegistrationRequest vm = new Services.Models.Request.NormalUserRegistrationRequest();
            String userEmail = email.Trim();
            ApplicationUser usr = await this._userManager.FindByEmailAsync(userEmail);

            vm.Email = userEmail;
            vm.UserDispName = usr.UserDispName;
            vm.container_id = usr.container_id;
            vm.PhoneNumber = usr.PhoneNumber;
            vm.Discriminator = usr.Discriminator;
            vm.user_visibility = usr.user_visibility;

            return this.View("~/Views/Account/NormalUserEdit.cshtml", vm);
        }

        /*
         * ユーザ情報の変更　処理
         */
        [HttpPost]
        public async Task<IActionResult> NormalUserEditConfirm(Services.Models.Request.NormalUserRegistrationRequest vm)
        {
            try
            {
                // コンテナ管理者のみ利用可能な機能
                var loginUser = await this._userManager.FindByIdAsync(this.UserId);
                if (loginUser.Discriminator != "ContainerAdmin")
                {
                    throw new Exception("Access Denied");
                }
                if (ModelState.IsValid)
                {
                    vm.Email = vm.Email.Trim();
                    var user = await _userManager.FindByEmailAsync(vm.Email);
                    if (user != null)
                    {
                        user.Email = vm.Email;
                        user.UserDispName = vm.UserDispName;
                        user.PhoneNumber = vm.PhoneNumber;
                        user.user_visibility = vm.user_visibility;
                        user.Discriminator = vm.Discriminator == "一般" ? "ApplicationUser" : "ContainerAdmin";
                    }
                    var update = await this._userManager.UpdateAsync(user);
                    if (update.Succeeded)
                    {
                        TempData["custom-validation-success-message"] = "User has been successfully edited!";
                        return Redirect("/Account/NormalUserList");
                    }
                    throw new Exception(update.Errors.FirstOrDefault().Description);
                }
                else
                {
                    var errMsgs = ModelState.SelectMany(c => c.Value.Errors);
                    throw new Exception(errMsgs.First().ErrorMessage);
                    return this.View("~/Views/Account/NormalUserEdit.cshtml", vm.Email);
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                return View("~/Views/Error/204.cshtml");
            }

            return Redirect("/Account/NormalUserList");
        }

        // END ------------------------ コンテナ管理者＞ユーザ一覧/登録/変更/削除 ------------------------
        
    }
}
