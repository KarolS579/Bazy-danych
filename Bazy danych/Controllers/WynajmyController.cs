using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    public class WynajmyController : Controller
    {
        // STATYCZNE LISTY - Nasz tymczasowy magazyn dla wynajmów, klientów i sprzętu
        private static List<Klient> sztuczniKlienci = new List<Klient>()
        {
            new Klient { Id = 1, Nazwisko = "Jan Kowalski" },
            new Klient { Id = 2, Nazwisko = "Piotr Nowak - Firma" }
        };

        private static List<Sprzet> sztucznySprzet = new List<Sprzet>()
        {
            new Sprzet { Id = 1, Nazwa = "Wiertarka udarowa Bosch" },
            new Sprzet { Id = 2, Nazwa = "Zagęszczarka do gruntu" }
        };

        private static List<Wynajem> makietaWynajmow = new List<Wynajem>()
        {
            new Wynajem
            {
                Id = 1,
                DataWypozyczenia = DateTime.Now.AddDays(-7),
                DataZwrotu = DateTime.Now.AddDays(-2),
                KlientId = 1,
                Klient = sztuczniKlienci[0],
                SprzetId = 1,
                Sprzet = sztucznySprzet[0]
            }
        };

        public ActionResult Index()
        {
            return View(makietaWynajmow);
        }

        public ActionResult Create()
        {
            ViewBag.KlientId = new SelectList(sztuczniKlienci, "Id", "Nazwisko");
            ViewBag.SprzetId = new SelectList(sztucznySprzet, "Id", "Nazwa");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Wynajem wynajem)
        {
            if (ModelState.IsValid)
            {
                wynajem.Id = makietaWynajmow.Count > 0 ? makietaWynajmow.Max(w => w.Id) + 1 : 1;

                wynajem.Klient = sztuczniKlienci.FirstOrDefault(k => k.Id == wynajem.KlientId);
                wynajem.Sprzet = sztucznySprzet.FirstOrDefault(s => s.Id == wynajem.SprzetId);

               
                makietaWynajmow.Add(wynajem);

                return RedirectToAction("Index");
            }

            ViewBag.KlientId = new SelectList(sztuczniKlienci, "Id", "Nazwisko", wynajem.KlientId);
            ViewBag.SprzetId = new SelectList(sztucznySprzet, "Id", "Nazwa", wynajem.SprzetId);
            return View(wynajem);
        }
    }
}