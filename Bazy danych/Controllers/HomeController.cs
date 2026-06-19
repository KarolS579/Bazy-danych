using Bazy_danych.Models;
using Bazy_Danych.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Bazy_danych.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            // Bezpośrednie zapytania SQL zapobiegają błędowi 'Invalid object name'
            var stats = new Home
            {
                // 1. Łączna liczba sprzętu w systemie
                TotalEquipment = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Sprzets]").FirstOrDefault(),

                // 2. Liczba sprzętu o statusie "Wynajęty"
                ActiveRentals = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Sprzets] WHERE [Status] = 'Wynajęty'").FirstOrDefault(),

                // 3. Liczba sprzętu o statusie "Serwis"
                TotalInService = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Sprzets] WHERE [Status] = 'Serwis'").FirstOrDefault(),

                // 4. Łączna liczba klientów
                TotalClients = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Klients]").FirstOrDefault(),

                // 5. Łączna liczba magazynów
                TotalWarehouses = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Magazyns]").FirstOrDefault()
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