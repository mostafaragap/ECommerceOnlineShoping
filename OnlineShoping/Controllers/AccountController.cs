using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OnlineShoping.DAL;
using OnlineShoping.Models;
using OnlineShoping.Repository;

namespace OnlineShoping.Controllers
{
   
    public class AccountController : Controller
    {
        public GenericUnitofWork _unitOfWork = new GenericUnitofWork();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private dbMyOnlineShoppingEntities _DBEntity = new dbMyOnlineShoppingEntities();
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }
        public string Encrypt(string str)
        {
            string EncrptKey = "2013;[pnuLIT)WebCodeExpert";
            byte[] byKey = { };
            byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
            byKey = System.Text.Encoding.UTF8.GetBytes(EncrptKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(str);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string str)
        {
            str = str.Replace(" ", "+");
            string DecryptKey = "2013;[pnuLIT)WebCodeExpert";
            byte[] byKey = { };
            byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
            byte[] inputByteArray = new byte[str.Length];

            byKey = System.Text.Encoding.UTF8.GetBytes(DecryptKey.Substring(0, 8));
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            inputByteArray = Convert.FromBase64String(str.Replace(" ", "+"));
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            System.Text.Encoding encoding = System.Text.Encoding.UTF8;
            return encoding.GetString(ms.ToArray());
        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email , string password)
        {
            var newPasswordHashed = Encrypt(password);

            Tbl_Members user =_unitOfWork.GetRepositoryInstance<Tbl_Members>().GetFirstOrDefaultByParameter(a=>a.EmailId==email && a.Password== newPasswordHashed);

           
            if(user!=null && user.UserType == 0)
            {
                if(user.IsActive == true && user.IsDelete==false)
                {
                    FormsAuthentication.SetAuthCookie(user.FristName,true);
                    Session["UserId"] = user.MemberId;
                    Session["UserName"] = user.FristName;
                    RedirectToAction("Index", "Home");
                }
                else if(user.IsActive == false)
                {
                    ViewBag.Active = "Sorry you are blocked , please contact the admin";
                    return View(user);
                }

            }
            else if (user != null && user.UserType == 1)
            {
                FormsAuthentication.SetAuthCookie(user.FristName, true);
                Session["AdminId"] = user.MemberId;
                Session["AdminName"] = user.FristName;
                return RedirectToAction("Dashboard", "Admin");


            }
            else
            {
                ViewBag.Active = "Invalid Login attempt";
                return View(user);
            }

            return RedirectToAction("Index", "Home");

        }

        
        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model , string confirmPaswword)
        {


            if (model.Password == confirmPaswword)
            {

                var user = new Tbl_Members { FristName = model.FristName, LastName = model.LastName, EmailId = model.EmailId, CreatedOn = DateTime.Now, IsActive = true, IsDelete = false };
                var newPasswordHashed = Encrypt(model.Password);
                user.Password = newPasswordHashed;
                _unitOfWork.GetRepositoryInstance<Tbl_Members>().Add(user);
                Session["UserId"] = model.MemberId;
                Session["UserName"] = model.FristName;
                return RedirectToAction("Index", "Home");

            }
            else
            {
                ViewBag.Message = "Two Passwords doesn't match";

                  return View(model);
            }
        

        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }
        
        // POST: /Account/LogOff
       

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }


        public ActionResult EditProfile()
        {
            if(Session["UserId"]!= null)
            {
                int UserId = (int)Session["UserId"];
                var user = _DBEntity.Tbl_Members.Where(a => a.MemberId == UserId).SingleOrDefault();
                Tbl_Members profile = new Tbl_Members();

                profile.FristName = user.FristName;
                profile.LastName = user.LastName;
                profile.EmailId = user.EmailId;
                return View(profile);
            }
            else if(Session["AdminId"] != null)
            {
                int UserId = (int)Session["AdminId"];
                var user = _DBEntity.Tbl_Members.Where(a => a.MemberId == UserId).SingleOrDefault();
                Tbl_Members profile = new Tbl_Members();

                profile.FristName = user.FristName;
                profile.LastName = user.LastName;
                profile.EmailId = user.EmailId;
                return View(profile);
            }
            return View();
               
          

            
        }

        [HttpPost]
        public ActionResult EditProfile(Tbl_Members profile , string CurrPassword , string NewPaswword , string ConfirmPassword )
        {
            if(Session["UserId"]!= null)
            {
                int UserId = (int)Session["UserId"];

                var CurrentUser = _DBEntity.Tbl_Members.Where(a => a.MemberId == UserId).SingleOrDefault();
                string OldPass = Decrypt(CurrentUser.Password);

                if (NewPaswword != ConfirmPassword)
                {
                    ViewBag.MessageEdit = "Two Passwords are not match";
                }

                else if (CurrPassword != OldPass)
                {

                    ViewBag.MessageEdit = "The old password is invalid";


                }

                else
                {
                    string NewPass = Encrypt(NewPaswword);

                    CurrentUser.FristName = profile.FristName;
                    CurrentUser.LastName = profile.LastName;
                    CurrentUser.EmailId = profile.EmailId;
                    CurrentUser.Password = NewPass;
                    CurrentUser.ModifiedOn = DateTime.Now;
                    CurrentUser.IsDelete = false;
                    CurrentUser.IsActive = true;
                    _DBEntity.Entry(CurrentUser).State = System.Data.Entity.EntityState.Modified;
                    _DBEntity.SaveChanges();
                    ViewBag.MessageEdit = "Account has modified succesfully";
                }

                return View(profile);
            }else if(Session["AdminId"] != null)
            {

                int UserId = (int)Session["AdminId"];

                var CurrentUser = _DBEntity.Tbl_Members.Where(a => a.MemberId == UserId).SingleOrDefault();
                string OldPass = Decrypt(CurrentUser.Password);

                if (NewPaswword != ConfirmPassword)
                {
                    ViewBag.MessageEdit = "Two Passwords are not match";
                }

                else if (CurrPassword != OldPass)
                {

                    ViewBag.MessageEdit = "The old password is invalid";


                }

                else
                {
                    string NewPass = Encrypt(NewPaswword);

                    CurrentUser.FristName = profile.FristName;
                    CurrentUser.LastName = profile.LastName;
                    CurrentUser.EmailId = profile.EmailId;
                    CurrentUser.Password = NewPass;
                    CurrentUser.ModifiedOn = DateTime.Now;
                    CurrentUser.IsDelete = false;
                    CurrentUser.IsActive = true;
                    _DBEntity.Entry(CurrentUser).State = System.Data.Entity.EntityState.Modified;
                    _DBEntity.SaveChanges();
                    ViewBag.MessageEdit = "Account has modified succesfully";
                }

                return View(profile);
            }

            return View(profile);
        }
        #endregion
    }
}