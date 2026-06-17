using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Bazy_danych.Models;
using Bazy_Danych.Models;

namespace Bazy_danych.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            // Make sure your database context matches your model names
            var stats = new Home
            {
                // 1. Total number of equipment pieces in the system
                TotalEquipment = db.Sprzet.Count(),

                // 2. Count ONLY items where the status string is exactly "Wynajęty"
                ActiveRentals = db.Sprzet.Count(s => s.Status == "Wynajęty"),

                // 3. Count ONLY items where the status string is exactly "W serwisie"
                TotalInService = db.Sprzet.Count(s => s.Status == "Serwis"),

                // 4. Total number of active clients
                TotalClients = db.Klienci.Count(),

                TotalWarehouses = db.Magazyny.Count()
            };

            return View(stats);
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

        public ActionResult Customers()
        {
            return View();
        }


        public ActionResult Klienci()
        {
            return View();
        }

        public ActionResult Rentals()
        {
            return View();
        }

        public ActionResult Services()
        {
            return View();
        }

        public ActionResult Panel()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

