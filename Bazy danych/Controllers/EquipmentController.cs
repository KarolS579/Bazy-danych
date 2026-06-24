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
    public class EquipmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static readonly string[] EquipmentStatuses = new[] { "Dostępny", "Wynajęty", "Serwis", "Archiwalny" };
        public ActionResult Equipment()
        {
            var items = db.Sprzets.OrderByDescending(x => x.CreatedDate).ToList();
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
                db.Sprzets.Add(model);
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

            var item = db.Sprzets.Find(id.Value);
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
                var item = db.Sprzets.Find(model.Id);
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
        public JsonResult DeleteJson(int id)
        {
            var sprzet = db.Sprzets.Find(id);
            if (sprzet == null)
            {
                return Json(new { success = false, message = "Nie znaleziono sprzętu." });
            }

            // --- KLUCZOWA ZMIANA 1: Status Archiwalny zawsze pomija blokady ---
            if (sprzet.Status == "Archiwalny")
            {
                try
                {
                    var powiazaneSerwisy = db.Serwisy.Where(s => s.SprzetId == id);
                    db.Serwisy.RemoveRange(powiazaneSerwisy);

                    var powiazaneWynajmy = db.Wynajmy.Where(w => w.SprzetId == id);
                    db.Wynajmy.RemoveRange(powiazaneWynajmy);

                    db.Sprzets.Remove(sprzet);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Błąd podczas usuwania zarchiwizowanego sprzętu: " + ex.Message });
                }
            }

            // --- KLUCZOWA ZMIANA 2: Najwyższy priorytet mają AKTUALNE operacje (Wynajem i Serwis) ---

            // 1. Sprawdzamy czy jest AKTUALNIE WYNAJĘTY (to musi być na samym górze!)
            bool maWynajmy = db.Wynajmy.Any(w => w.SprzetId == id) || sprzet.Status == "Wynajęty";
            if (maWynajmy)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie można usunąć tego sprzętu, ponieważ posiada aktywne wynajmy w bazie danych!"
                });
            }

            // 2. Sprawdzamy czy AKTUALNIE PRZEBYWA W SERWISIE (brak daty zakończenia)
            bool jestAktualnieWSerwisie = db.Serwisy.Any(s => s.SprzetId == id && s.DataZakonczenia == null)
                                         || sprzet.Status == "Serwis";

            if (jestAktualnieWSerwisie)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie można usunąć tego sprzętu, ponieważ aktualnie przebywa on w serwisie!"
                });
            }

            // --- KLUCZOWA ZMIANA 3: Dopiero na samym końcu sprawdzamy STARYCH, zamkniętych serwisantów ---

            // 3. Sprawdzamy czy posiada JAKĄKOLWIEK HISTORIĘ SERWISOWĄ
            bool posiadaHistorieSerwisowa = db.Serwisy.Any(s => s.SprzetId == id);
            if (posiadaHistorieSerwisowa)
            {
                return Json(new
                {
                    success = false,
                    message = "Ten sprzęt posiada historię serwisową i nie może zostać usunięty ze względów referencyjnych."
                });
            }

            try
            {
                db.Sprzets.Remove(sprzet);
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie można usunąć tego sprzętu z powodu powiązań referencyjnych w bazie danych."
                });
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