using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize]
    public class SerwisyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult Index()
        {
            var serwisy = db.Serwisy.Include(s => s.Sprzet).ToList();
            return View(serwisy);
        }


        public ActionResult Create()
        {
            ViewBag.SprzetId = db.Sprzets
                .Where(s => s.Status == "Dostępny") // Pobieramy tylko sprzęty gotowe do wypożyczenia/serwisu
                .ToList() // Pobieramy dane do pamięci, aby móc sformatować tekst w C#
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    // Łączymy numer seryjny z nazwą w jeden ciąg tekstowy
                    Text = $"[{s.NumerSeryjny}] {s.Nazwa} ({s.Kategoria})"
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DataRozpoczecia,Opis,SprzetId")] Serwis serwis)
        {
            if (ModelState.IsValid)
            {
                var sprzet = db.Sprzets.Find(serwis.SprzetId);

                if (sprzet != null && sprzet.Status == "Dostępny")
                {
                    sprzet.Status = "Serwis";
                    db.Entry(sprzet).State = EntityState.Modified;

                    db.Serwisy.Add(serwis);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Ten sprzęt nie jest dostępny i nie może zostać wysłany na serwis.");
                }
            }

            ViewBag.SprzetId = new SelectList(db.Sprzets.Where(s => s.Status == "Dostępny"), "Id", "Nazwa", serwis.SprzetId);
            return View(serwis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PowrotZSerwisu(int id)
        {
            var serwis = db.Serwisy.Find(id);
            if (serwis != null)
            {
                var sprzet = db.Sprzets.Find(serwis.SprzetId);
                if (sprzet != null)
                {
                    sprzet.Status = "Dostępny";
                    db.Entry(sprzet).State = EntityState.Modified;
                }

                serwis.DataZakonczenia = DateTime.Now;
                db.Entry(serwis).State = EntityState.Modified;

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}