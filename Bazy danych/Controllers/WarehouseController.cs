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
            if (ModelState["ZajeteMiejsce"] != null && ModelState["ZajeteMiejsce"].Errors.Count > 0)
            {
                ModelState["ZajeteMiejsce"].Errors.Clear();
                ModelState.AddModelError("ZajeteMiejsce", "Wprowadzona pojemność jest zbyt duża lub nieprawidłowa! Maksymalna liczba to 2147483647");
            }

            if (ModelState["Pojemnosc"] != null && ModelState["Pojemnosc"].Errors.Count > 0)
            {
                ModelState["Pojemnosc"].Errors.Clear();
                ModelState.AddModelError("Pojemnosc", "Wprowadzona pojemność jest zbyt duża lub nieprawidłowa! Maksymalna liczba to 2147483647");
            }

            if (ModelState.IsValid)
            {
                bool nazwaIstnieje = db.Magazyny.Any(m => m.Nazwa.Equals(model.Nazwa, StringComparison.OrdinalIgnoreCase));

                if (nazwaIstnieje)
                {
                    ModelState.AddModelError("Duplikat", "Magazyn o podanej nazwie już istnieje w bazie danych!");
                    return View(model);
                }

                if (model.ZajeteMiejsce > model.Pojemnosc)
                {
                    ModelState.AddModelError("ZajeteMiejsce", "Zajęte miejsce nie może być większe niż pojemność.");
                    return View(model);
                }

                model.CreatedDate = DateTime.Now;
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
            if (ModelState["ZajeteMiejsce"] != null && ModelState["ZajeteMiejsce"].Errors.Count > 0)
            {
                ModelState["ZajeteMiejsce"].Errors.Clear();
                ModelState.AddModelError("ZajeteMiejsce", "Wprowadzona pojemność jest zbyt duża lub nieprawidłowa! Maksymalna liczba to 2147483647");
            }

            if (ModelState["Pojemnosc"] != null && ModelState["Pojemnosc"].Errors.Count > 0)
            {
                ModelState["Pojemnosc"].Errors.Clear();
                ModelState.AddModelError("Pojemnosc", "Wprowadzona pojemność jest zbyt duża lub nieprawidłowa! Maksymalna liczba to 2147483647");
            }

            if (ModelState.IsValid)
            {
                if (model.ZajeteMiejsce > model.Pojemnosc)
                {
                    ModelState.AddModelError("ZajeteMiejsce", "Zajęte miejsce nie może być większe niż pojemność.");
                    return View(model);
                }

                var warehouse = db.Magazyny.Find(model.Id);
                if (warehouse == null)
                {
                    return HttpNotFound();
                }

                warehouse.Nazwa = model.Nazwa;
                warehouse.Lokalizacja = model.Lokalizacja;
                warehouse.Pojemnosc = model.Pojemnosc;
                warehouse.ZajeteMiejsce = model.ZajeteMiejsce;
                warehouse.Status = model.Status;

                db.SaveChanges();
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

