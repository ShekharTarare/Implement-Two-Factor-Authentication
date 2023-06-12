using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;

namespace ImplementTwoFactorAuth.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (IsValidUser(username, password))
            {
                FormsAuthentication.SetAuthCookie(username, false);
                return RedirectToAction("ShowAuthenticationTokenPage", "Account");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password";
                return View();
            }
        }

        public ActionResult ShowAuthenticationTokenPage()
        {
            string googleAuthKey = WebConfigurationManager.AppSettings["GoogleAuthKey"];

            //Two Factor Authentication Setup
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            var setupInfo = TwoFacAuth.GenerateSetupCode("Shekhartarare.com", "Admin", ConvertSecretToBytes(googleAuthKey, false), 300);

            ViewBag.BarcodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            ViewBag.SetupCode = setupInfo.ManualEntryKey;
            return View();
        }

        [HttpPost]
        public ActionResult AuthenticateToken()
        {
            var token = Request["EnteredCode"];
            bool isValid = ValidateToken(token);
            if (isValid)
            {
                return RedirectToAction("Index", "Home"); // Redirect to default landing page
            }
            else
                return RedirectToAction("Logout");
        }

        public bool ValidateToken(string token)
        {
            string googleAuthKey = WebConfigurationManager.AppSettings["GoogleAuthKey"];
            // Adjust the validation window to allow a time skew of 1 step (30 seconds)
            int validationWindow = 1;
            TwoFactorAuthenticator TwoFacAuth = new TwoFactorAuthenticator();
            return TwoFacAuth.ValidateTwoFactorPIN(googleAuthKey, token, validationWindow);
        }

        private static byte[] ConvertSecretToBytes(string secret, bool secretIsBase32) =>
           secretIsBase32 ? Base32Encoding.ToBytes(secret) : Encoding.UTF8.GetBytes(secret);

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        private bool IsValidUser(string username, string password)
        {
            return FormsAuthentication.Authenticate(username, password);
        }
    }
}