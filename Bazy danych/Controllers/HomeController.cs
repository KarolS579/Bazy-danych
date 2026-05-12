using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Bazy_danych.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Wprowadź adres e-mail i hasło.";
                return View();
            }

            try
            {
                var parsedEmail = new MailAddress(email);
            }
            catch (FormatException)
            {
                ViewBag.ErrorMessage = "Wprowadź poprawny adres e-mail.";
                return View();
            }

            return RedirectToAction("Index");
        }
    }
}
