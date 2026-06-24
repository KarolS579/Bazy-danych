using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize] // Opcjonalnie: dostęp tylko dla zalogowanych pracowników/adminów
    public class WynajmyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // 1. LISTA WYNAJMÓW (Index)
        public ActionResult Index()
        {
            // .Include automatycznie dołącza powiązane dane klienta i sprzętu w jednym zapytaniu SQL
            var wynajmy = db.Wynajmy.Include(w => w.Klient).Include(w => w.Sprzets).ToList();
            return View(wynajmy);
        }

        // 2. FORMULARZ DODAWANIA (GET)
        public ActionResult Create()
        {
            // Czyścimy pamięć podręczną kontekstu, aby na pewno pobrać nowy sprzęt po reinstalacji w bazie
            db.Configuration.ProxyCreationEnabled = false;

            // Pobieramy tylko te sprzęty, które NIE są aktualnie wynajęte ani w serwisie
            var dostepneSprzety = db.Sprzets
                                    .AsNoTracking() // Wymuszamy bezpośrednie zapytanie do bazy danych (omijamy Cache)
                                    .Where(s => s.Status == "Dostępny")
                                    .ToList();

            ViewBag.SprzetId = new SelectList(dostepneSprzety, "Id", "Nazwa");

            var listaKlientow = db.Klienci.AsNoTracking().ToList().Select(k => new {
                Id = k.Id,
                PelneDane = k.Imie + " " + k.Nazwisko
            });
            ViewBag.KlientId = new SelectList(listaKlientow, "Id", "PelneDane");

            return View();
        }

        // 3. ZAPIS WYNAJMU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DataWynajmu,DataZwrotu,SprzetId,KlientId")] Wynajem wynajem)
        {
            if (ModelState.IsValid)
            {
                // Wszystkie if-y sprawdzające sprzęt i modyfikujące EntityState.Modified zostają usunięte.
                // Trigger w bazie danych wykona to automatycznie po wywołaniu SaveChanges().

                db.Wynajmy.Add(wynajem);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Kod w przypadku błędu walidacji (pozostaje bez zmian)
            ViewBag.SprzetId = new SelectList(db.Sprzets.Where(s => s.Status != "Wynajęte"), "Id", "Nazwa", wynajem.SprzetId);
            var listaKlientow = db.Klienci.ToList().Select(k => new { Id = k.Id, PelneDane = k.Imie + " " + k.Nazwisko });
            ViewBag.KlientId = new SelectList(listaKlientow, "Id", "PelneDane", wynajem.KlientId);
            return View(wynajem);
        }
        // 4. USUWANIE WYNAJMU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var wynajem = db.Wynajmy.Find(id);

            if (wynajem != null)
            {
                var sprzet = db.Sprzets.Find(wynajem.SprzetId);
                if (sprzet != null)
                {
                    sprzet.Status = "Dostępny";
                    db.Entry(sprzet).State = EntityState.Modified;
                }

                db.Wynajmy.Remove(wynajem);

                db.SaveChanges();
            }

            return RedirectToAction("Index");
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