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
    public class EquipmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static readonly string[] EquipmentStatuses = new[] { "Dostępny", "Wynajęty", "Serwis" };
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