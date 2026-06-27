using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Warehouses()
        {
            var warehouses = db.Magazyny.OrderByDescending(x => x.CreatedDate).ToList();
            return View(warehouses);
        }

        public ActionResult AddWarehouse()
        {
            return View(new Magazyn());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddWarehouse(Magazyn model)
        {
            // Czyścimy błędy walidacji dla pola ZajeteMiejsce, ponieważ zawsze przy dodawaniu wynosi 0
            if (ModelState["ZajeteMiejsce"] != null)
            {
                ModelState["ZajeteMiejsce"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                model.ZajeteMiejsce = 0; // Nowy magazyn jest zawsze pusty
                model.CreatedDate = DateTime.Now;
                model.Status = "Aktywny";

                db.Magazyny.Add(model);
                db.SaveChanges();
                return RedirectToAction("Warehouses");
            }
            return View(model);
        }

        public ActionResult EditWarehouse(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var warehouse = db.Magazyny.Find(id.Value);
            if (warehouse == null)
            {
                return HttpNotFound();
            }

            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditWarehouse(Magazyn model)
        {
            if (ModelState.IsValid)
            {
                // Liczymy rzeczywistą liczbę sprzętów podpiętych pod ten magazyn
                int rzeczywistaLiczbaEq = db.Sprzets.Count(s => s.MagazynId == model.Id);

                // UI ERROR: Próba zmniejszenia pojemności poniżej liczby posiadanych maszyn
                if (model.Pojemnosc < rzeczywistaLiczbaEq)
                {
                    ModelState.AddModelError("Pojemnosc", $"Nie można ustawić pojemności mniejszej niż liczba powiązanych sprzętów! Aktualna ilość sprzętów w tym magazynie: {rzeczywistaLiczbaEq}.");
                    return View(model);
                }

                var warehouse = db.Magazyny.Find(model.Id);
                if (warehouse == null) return HttpNotFound();

                warehouse.Nazwa = model.Nazwa;
                warehouse.Lokalizacja = model.Lokalizacja;
                warehouse.Pojemnosc = model.Pojemnosc;
                warehouse.ZajeteMiejsce = rzeczywistaLiczbaEq; // Wymuszenie aktualnej, wyliczonej wartości

                db.SaveChanges(); // Odpala trigger i aktualizuje status (Aktywny/Przepełniony)
                return RedirectToAction("Warehouses");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteWarehouse(int id)
        {
            var warehouse = db.Magazyny.Find(id);
            if (warehouse == null)
            {
                return HttpNotFound();
            }

            db.Magazyny.Remove(warehouse);
            db.SaveChanges();

            return RedirectToAction("Warehouses");
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

