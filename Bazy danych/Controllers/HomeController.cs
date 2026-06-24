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
            // Komplet zapytania całkowicie odpornego na kodowanie znaków i typy danych typu NCHAR
            var stats = new Home
            {
                // 1. Łączna liczba sprzętu w systemie
                TotalEquipment = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Sprzets]").FirstOrDefault(),

                // 2. BEZPIECZNE ZLICZANIE WYNAJĘTYCH (Łapie słowa: Wynajęty, Wynajety, Wynajęte, WYNAJĘTY itd.)
                ActiveRentals = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM [dbo].[Sprzets] WHERE UPPER(CAST([Status] AS NVARCHAR(MAX))) LIKE '%WYNAJ%'"
                ).FirstOrDefault(),

                // 3. BEZPIECZNE ZLICZANIE SERWISU
                TotalInService = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM [dbo].[Sprzets] WHERE UPPER(CAST([Status] AS NVARCHAR(MAX))) LIKE '%SERWIS%'"
                ).FirstOrDefault(),

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