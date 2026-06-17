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
    // Wipe out default low-level framework mapping complaints for individual text strings if they break completely
    string[] properties = { "Imie", "Nazwisko", "Telefon", "Email", "Adres", "Firma", "Uwagi" };
    foreach (var prop in properties)
    {
        if (ModelState[prop] != null && ModelState[prop].Errors.Count > 0)
        {
            // Only overwrite if the framework completely failed to bind the text stream layout
            var error = ModelState[prop].Errors.FirstOrDefault();
            if (error != null && (error.Exception != null || string.IsNullOrEmpty(error.ErrorMessage)))
            {
                ModelState[prop].Errors.Clear();
                ModelState.AddModelError(prop, "Wprowadzona wartość jest zbyt długa lub nieprawidłowa!");
            }
        }
    }

    // This will evaluate to 'false' if a user inputs numbers into Imie or Nazwisko
    if (ModelState.IsValid)
    {
        db.Klienci.Add(klient);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    // Returns back to the view showing your customized letter-only constraint message
    return View(klient);
}

        // 1. GET: Klienci/Edit/4 (Loads the page with the client's current data)
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Find the client in the real database using the ID from the URL
            Klient klient = db.Klienci.Find(id);

            if (klient == null)
            {
                return HttpNotFound(); // Returns a clean 404 if the client ID doesn't exist in SQL
            }

            return View(klient); // Passes the client data over to the Edit.cshtml view
        }

        // 2. POST: Klienci/Edit/4 (Saves the updated data back to SQL)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Klient klient)
        {
            // Clean up framework line-length string binding errors if necessary
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
                // Tell Entity Framework that this record already exists and needs to be updated
                db.Entry(klient).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges(); // Commit updates to the database

                return RedirectToAction("Index");
            }

            return View(klient); // If validation fails (e.g. numbers in name), stay on page with errors
        }

        // POST: Klienci/DeleteKlienci
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteKlienci(int id)
        {
            // This works perfectly now because it searches the same database tables!
            Klient klient = db.Klienci.Find(id);

            if (klient != null)
            {
                db.Klienci.Remove(klient);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose(); // Clears database connection pools safely
            }
            base.Dispose(disposing);
        }
    }
}