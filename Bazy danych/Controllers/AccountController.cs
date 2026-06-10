using System;
using System.Linq;
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

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (userManager.FindByEmail(model.Email) != null)
                {
                    ModelState.AddModelError("", "Użytkownik z tym adresem e-mail już istnieje.");
                    return View(model);
                }

                var existingPending = db.PendingRegistrations.FirstOrDefault(x => x.Email == model.Email);
                if (existingPending != null)
                {
                    var secondsSinceLastSend = (DateTime.UtcNow - existingPending.CreatedAtUtc).TotalSeconds;
                    if (secondsSinceLastSend < 30)
                    {
                        var waitSeconds = (int)Math.Ceiling(30 - secondsSinceLastSend);
                        ModelState.AddModelError("", "Mail potwierdzający został już wysłany. Spróbuj ponownie za " + waitSeconds + " s.");
                        return View(model);
                    }

                    db.PendingRegistrations.Remove(existingPending);
                    db.SaveChanges();
                }

                var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                var pending = new PendingRegistration
                {
                    Email = model.Email,
                    PasswordHash = userManager.PasswordHasher.HashPassword(model.Password),
                    ConfirmationToken = token,
                    CreatedAtUtc = DateTime.UtcNow,
                    ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
                };

                db.PendingRegistrations.Add(pending);
                db.SaveChanges();

                try
                {
                    var callbackUrl = Url.Action("ConfirmPendingRegistration", "Account",
                        new { token = pending.ConfirmationToken }, protocol: Request.Url.Scheme);

                    await new EmailService().SendAsync(new IdentityMessage
                    {
                        Destination = pending.Email,
                        Subject = "Confirm your account",
                        Body = "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>"
                    });

                    return View("DisplayEmail");
                }
                catch
                {
                    db.PendingRegistrations.Remove(pending);
                    db.SaveChanges();
                    ModelState.AddModelError("", "Nie udało się wysłać maila potwierdzającego. Konto nie zostało utworzone. Sprawdź konfigurację SMTP i spróbuj ponownie.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmPendingRegistration(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new HttpStatusCodeResult(400);
            }

            var pending = db.PendingRegistrations.FirstOrDefault(x => x.ConfirmationToken == token);
            if (pending == null || pending.ExpiresAtUtc < DateTime.UtcNow)
            {
                return new HttpStatusCodeResult(400);
            }

            if (userManager.FindByEmail(pending.Email) != null)
            {
                db.PendingRegistrations.Remove(pending);
                db.SaveChanges();
                return View("ConfirmEmail");
            }

            var user = new ApplicationUser { UserName = pending.Email, Email = pending.Email, EmailConfirmed = true };
            var createResult = userManager.Create(user);
            if (!createResult.Succeeded)
            {
                return new HttpStatusCodeResult(400);
            }

            user.PasswordHash = pending.PasswordHash;
            db.SaveChanges();

            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }
            userManager.AddToRole(user.Id, "User");

            db.PendingRegistrations.Remove(pending);
            db.SaveChanges();

            await Task.CompletedTask;
            return View("ConfirmEmail");
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

        public ActionResult Login()
        {
            return View();
        }

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

        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

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

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var identity = userManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, identity);
            await Task.CompletedTask;
        }

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
