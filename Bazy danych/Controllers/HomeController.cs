using Bazy_danych.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Bazy_danych.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            if (Request.IsAuthenticated && !User.IsInRole("Admin"))
            {
                return RedirectToAction("UserIndex");
            }

            // Statystyki dla administratora
            var stats = new Home
            {
                // Używamy nazw tabel zgodnych z bazą danych (EF domyślnie dodaje 's' na końcu)
                TotalEquipment = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Sprzets]").FirstOrDefault(),

                ActiveRentals = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM [dbo].[Sprzets] WHERE UPPER(CAST([Status] AS NVARCHAR(MAX))) LIKE '%WYNAJ%'"
                ).FirstOrDefault(),

                TotalInService = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM [dbo].[Sprzets] WHERE UPPER(CAST([Status] AS NVARCHAR(MAX))) LIKE '%SERWIS%'"
                ).FirstOrDefault(),

                TotalClients = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Klients]").FirstOrDefault(),

                TotalWarehouses = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM [dbo].[Magazyns]").FirstOrDefault()
            };

            return View(stats);
        }

[Authorize]
public ActionResult UserIndex()
{
    // Ochrona: Jeśli admin tu wejdzie, przekierowujemy do panelu administracyjnego
    if (User.IsInRole("Admin"))
    {
        return RedirectToAction("Index");
    }

    int aktywneWynajmyCount = 0;
    // Pobieramy e-mail zalogowanego użytkownika i standaryzujemy go (brak spacji, małe litery)
    string currentEmail = (User.Identity.Name ?? "").Trim().ToLower();

    try
    {
        // 1. Pobieramy listę klientów z db.Klienci do pamięci aplikacji
        var wszyscyKlienci = db.Klienci.ToList();

        // 2. Szukamy profilu, w którym Email pasuje do loginu użytkownika
        var profilKlienta = wszyscyKlienci.FirstOrDefault(k => 
            (k.Email ?? "").Trim().ToLower() == currentEmail);

        if (profilKlienta != null)
        {
            // 3. Sukces! Znamy liczbowe Id klienta. Zliczamy umowy w db.Wynajmy, które nie mają wpisanej daty zwrotu
            aktywneWynajmyCount = db.Wynajmy
                .Count(w => w.KlientId == profilKlienta.Id && w.DataZwrotu == null);
        }
    }
    catch (Exception)
    {
        aktywneWynajmyCount = 0;
    }

    // Przekazanie gotowej wartości do kafelka w widoku UserIndex.cshtml
    ViewBag.AktywneWynajmy = aktywneWynajmyCount;
    ViewBag.UserEmail = User.Identity.Name;

    return View();
}

        public ActionResult About()
        {
            ViewBag.Message = "Strona o nas.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Strona kontaktowa.";
            return View();
        }

        // Pozostałe metody bez zmian...
        public ActionResult Customers() { return View(); }
        public ActionResult Klienci() { return View(); }
        public ActionResult Rentals() { return View(); }
        public ActionResult Services() { return View(); }
        public ActionResult Panel() { return View(); }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}