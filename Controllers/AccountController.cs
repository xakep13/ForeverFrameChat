using ForeverFrameChat.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;

namespace ForeverFrameChat.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser { UserName = model.Email, Email = model.Email, Login = model.Login };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return Redirect("Login");
                }
                else
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            return View(model);
        }

        public ActionResult Login(string returnUrl)
        {
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl)
        {
            return Redirect(returnUrl);
            //if (ModelState.IsValid)
            //{
            //    ApplicationUser user = await UserManager.FindAsync(model.Login, model.Password);
            //    if (user == null)
            //    {
            //        ModelState.AddModelError("", "Невірний логін або пароль.");
            //    }
            //    else
            //    {
            //        ClaimsIdentity claim = await UserManager.CreateIdentityAsync(user,
            //                                DefaultAuthenticationTypes.ApplicationCookie);
            //        AuthenticationManager.SignOut();
            //        AuthenticationManager.SignIn(new AuthenticationProperties
            //        {
            //            IsPersistent = true
            //        }, claim);
            //        if (String.IsNullOrEmpty(returnUrl))
            //            return Redirect("/Home/Index");
            //        return Redirect(returnUrl);
            //    }
            //}
            //ViewBag.returnUrl = returnUrl;
            //return View(model);
        }
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return Redirect("Login");
        }
    }
}