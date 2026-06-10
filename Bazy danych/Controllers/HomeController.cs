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
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static readonly string[] EquipmentStatuses = new[] { "Dostępny", "Wynajęty", "Serwis" };

        public ActionResult Index()
        {
            return View();
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

        public ActionResult Equipment()
        {
            var items = db.Sprzet.OrderByDescending(x => x.CreatedDate).ToList();
            return View(items);
        }

        public ActionResult AddEquipment()
        {
            ViewBag.Statuses = new SelectList(EquipmentStatuses);
            return View(new Sprzet());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEquipment(Sprzet model)
        {
            model.Status = (model.Status ?? string.Empty).Trim();

            if (!EquipmentStatuses.Contains(model.Status))
            {
                ModelState.AddModelError("Status", "Wybierz poprawny status.");
            }

            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                db.Sprzet.Add(model);
                db.SaveChanges();
                return RedirectToAction("Equipment");
            }

            ViewBag.Statuses = new SelectList(EquipmentStatuses, model.Status);
            return View(model);
        }

        public ActionResult EditEquipment(int? id)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = db.Sprzet.Find(id.Value);
            if (item == null)
            {
                return HttpNotFound();
            }

            ViewBag.Statuses = new SelectList(EquipmentStatuses, item.Status);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEquipment(Sprzet model)
        {
            model.Status = (model.Status ?? string.Empty).Trim();

            if (!EquipmentStatuses.Contains(model.Status))
            {
                ModelState.AddModelError("Status", "Wybierz poprawny status.");
            }

            if (ModelState.IsValid)
            {
                var item = db.Sprzet.Find(model.Id);
                if (item == null)
                {
                    return HttpNotFound();
                }

                item.Nazwa = model.Nazwa;
                item.Kategoria = model.Kategoria;
                item.Cena_wynajmu = model.Cena_wynajmu;
                item.Status = model.Status;

                db.SaveChanges();
                return RedirectToAction("Equipment");
            }

            ViewBag.Statuses = new SelectList(EquipmentStatuses, model.Status);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEquipment(int id)
        {
            var item = db.Sprzet.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            db.Sprzet.Remove(item);
            db.SaveChanges();

            return RedirectToAction("Equipment");
        }

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
            if (ModelState.IsValid)
            {
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

        public ActionResult Customers()
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

