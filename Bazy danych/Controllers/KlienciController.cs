using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize]
    public class KlienciController : Controller
    {
        // This is your live bridge to the database
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Klienci
        public ActionResult Index()
        {
            // PULL FROM REAL DATABASE: Fetch actual records from SQL
            var listaKlientow = db.Klienci.ToList();
            return View(listaKlientow);
        }

        // GET: Klienci/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Klienci/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Klient klient)
        {
            string[] properties = { "Imie", "Nazwisko", "Telefon", "Email", "Adres", "Firma", "Uwagi" };
            foreach (var prop in properties)
            {
                if (ModelState[prop] != null && ModelState[prop].Errors.Count > 0)
                {
                    var error = ModelState[prop].Errors.FirstOrDefault();
                    if (error != null && (error.Exception != null || string.IsNullOrEmpty(error.ErrorMessage)))
                    {
                        ModelState[prop].Errors.Clear();
                        ModelState.AddModelError(prop, "Wprowadzona wartość jest zbyt długa lub nieprawidłowa!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.Klienci.Add(klient);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(klient);
        }

        // GET: Klienci/Edit/4
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            Klient klient = db.Klienci.Find(id);

            if (klient == null)
            {
                return HttpNotFound();
            }

            return View(klient);
        }

        // POST: Klienci/Edit/4
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Klient klient)
        {
            string[] properties = { "Imie", "Nazwisko", "Telefon", "Email", "Adres", "Firma", "Uwagi" };
            foreach (var prop in properties)
            {
                if (ModelState[prop] != null && ModelState[prop].Errors.Count > 0)
                {
                    var error = ModelState[prop].Errors.FirstOrDefault();
                    if (error != null && (error.Exception != null || string.IsNullOrEmpty(error.ErrorMessage)))
                    {
                        ModelState[prop].Errors.Clear();
                        ModelState.AddModelError(prop, "Wprowadzona wartość jest zbyt długa lub nieprawidłowa!");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(klient).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(klient);
        }

        // POST: Klienci/DeleteKlienci
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteKlienci(int id)
        {
            Klient klient = db.Klienci.Find(id);

            if (klient == null)
            {
                return Json(new { success = false, message = "Nie znaleziono klienta." });
            }

            // Sprawdzamy powiązanie z tabelą wynajmów
            bool maAktywneWynajmy = db.Wynajmy.Any(w => w.KlientId == id);

            if (maAktywneWynajmy)
            {
                // Zwracamy informację o blokadzie w formacie JSON bez usuwania
                return Json(new
                {
                    success = false,
                    message = "Nie można usunąć tego klienta, ponieważ posiada on aktywne wynajmy sprzętu!"
                });
            }

            try
            {
                db.Klienci.Remove(klient);
                db.SaveChanges();
                return Json(new { success = true }); // Sukces - usunięto
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Wystąpił błąd bazy danych podczas usuwania klienta." });
            }
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