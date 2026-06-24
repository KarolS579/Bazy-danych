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
            var stats = new Home
            {
                TotalEquipment = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM Sprzets").FirstOrDefault(),

                ActiveRentals = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM Sprzets WHERE Status = @p0", "Wynajęty").FirstOrDefault(),

                TotalInService = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM Sprzets WHERE Status = @p0", "Serwis").FirstOrDefault(),

                TotalClients = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM Klients").FirstOrDefault(),

                TotalWarehouses = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM Magazyns").FirstOrDefault()
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