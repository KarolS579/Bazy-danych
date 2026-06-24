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
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Klienci
        public ActionResult Index()
        {
            var listaKlientow = db.Database.SqlQuery<Klient>("SELECT * FROM Klients").ToList();
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
                string sql = "INSERT INTO Klients (Imie, Nazwisko, Telefon, Email, Adres, Firma, Uwagi) " +
                             "VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6)";

                db.Database.ExecuteSqlCommand(sql,
                    klient.Imie,
                    klient.Nazwisko,
                    klient.Telefon,
                    klient.Email,
                    klient.Adres,
                    klient.Firma,
                    klient.Uwagi
                );

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

            // Pobieranie pojedynczego klienta po ID przy użyciu parametru @p0
            Klient klient = db.Database.SqlQuery<Klient>("SELECT * FROM Klients WHERE Id = @p0", id).FirstOrDefault();

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
                string sql = "UPDATE Klients SET Imie = @p0, Nazwisko = @p1, Telefon = @p2, Email = @p3, " +
                             "Adres = @p4, Firma = @p5, Uwagi = @p6 WHERE Id = @p7";

                db.Database.ExecuteSqlCommand(sql,
                    klient.Imie,
                    klient.Nazwisko,
                    klient.Telefon,
                    klient.Email,
                    klient.Adres,
                    klient.Firma,
                    klient.Uwagi,
                    klient.Id // przekazujemy ID na samym końcu dla warunku WHERE (@p7)
                );

                return RedirectToAction("Index");
            }

            return View(klient);
        }

        // POST: Klienci/DeleteKlienci
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteKlienci(int id)
        {
            string sql = "DELETE FROM Klients WHERE Id = @p0";
            db.Database.ExecuteSqlCommand(sql, id);

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