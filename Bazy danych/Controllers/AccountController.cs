using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Bazy_danych.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;

namespace Bazy_danych.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        public AccountController()
        {
            db = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            userManager.EmailService = new EmailService();

            var provider = new DpapiDataProtectionProvider("Bazy_danych");
            userManager.UserTokenProvider =
                new DataProtectorTokenProvider<ApplicationUser>(provider.Create("ASP.NET Identity"));

            roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new user
                var user = new ApplicationUser{ UserName = model.Email, Email = model.Email };
                var result = userManager.Create(user, model.Password);
                if (result.Succeeded)
                {
                    if (!roleManager.RoleExists("User"))
                    {
                        roleManager.Create(new IdentityRole("User"));
                    }
                    userManager.AddToRole(user.Id, "User");

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    await userManager.SendEmailAsync(
                        user.Id,
                        "Confirm your account",
                        "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

                    return View("DisplayEmail");
                }
                else
                {
                    AddErrors(result);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return new HttpStatusCodeResult(400);
            }

            var result = await userManager.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }

            return new HttpStatusCodeResult(400);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {   
                var user = userManager.FindByEmail(model.Email);
                if (user != null && !user.EmailConfirmed)
                {
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    await userManager.SendEmailAsync(
                        user.Id,
                        "Confirm your account",
                        "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

                    return View("DisplayEmail");
                }

                else if (user != null && user.EmailConfirmed && userManager.CheckPassword(user, model.Password))
                {
                    await SignInAsync(user, model.RememberMe);
                    return RedirectToAction("Index", "Home");
                }

                else
                {
                    ModelState.AddModelError("", "Nieprawidłowy email lub hasło.");
                }
            }

            return View(model);
        }

        // GET: Account/LogOff
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        // POST: Admin/AddUserToRole
        // This is for admins to add an existing user to a role
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUserToRole(string userId, string roleName)
        {
            if (!roleManager.RoleExists(roleName))
            {
                roleManager.Create(new IdentityRole(roleName));
            }

            var user = userManager.FindById(userId);
            if (user != null && !userManager.IsInRole(userId, roleName))
            {
                userManager.AddToRole(userId, roleName);
            }

            return RedirectToAction("ManageUsers", "Admin");
        }

        // This is intentionally restricted so only an existing admin can create/assign admins.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new HttpStatusCodeResult(400, "Email and password are required.");
            }

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            var user = userManager.FindByEmail(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };

                var result = userManager.Create(user, password);
                if (!result.Succeeded)
                {
                    return new HttpStatusCodeResult(400, string.Join("; ", result.Errors));
                }
            }

            if (!userManager.IsInRole(user.Id, "Admin"))
            {
                userManager.AddToRole(user.Id, "Admin");
            }

            return new HttpStatusCodeResult(200, "Admin created or updated.");
        }

        // POST: Admin/RemoveAdmin
        // This is for admins to remove the admin role from a user
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveAdmin(string userId)
        {
            var user = userManager.FindById(userId);
            if (user == null)
            {
                return HttpNotFound();
            }

            if (userManager.IsInRole(userId, "Admin"))
            {
                userManager.RemoveFromRole(userId, "Admin");
            }

            return new HttpStatusCodeResult(200, "Admin role removed.");
        }

        // Helper method to sign in user
        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
            await Task.CompletedTask;
        }

        // Helper method to add errors
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && db != null)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
