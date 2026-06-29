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
    [Authorize(Roles = "Admin")]
    public class EquipmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private static readonly string[] EquipmentStatuses = new[] { "Dostępny", "Wynajęty", "Serwis", "Archiwalny" };
        public ActionResult Equipment()
        {
            var items = db.Sprzets.OrderByDescending(x => x.CreatedDate).ToList();
            return View(items);
        }

        private void PrepareMagazynDropdown(int? selectedId = null)
        {
            // Pobieramy magazyny
            var magazyny = db.Magazyny.ToList();
            var listItems = new List<SelectListItem>();

            foreach (var m in magazyny)
            {
                bool isFull = m.Status == "Zapełniony" || m.ZajeteMiejsce >= m.Pojemnosc;

                // Jeśli magazyn jest pełny, a NIE JEST to aktualnie wybrany magazyn edytowanego sprzętu -> blokujemy go
                bool shouldDisable = isFull && (selectedId == null || m.Id != selectedId);

                var item = new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Nazwa} (Zajęte: {m.ZajeteMiejsce}/{m.Pojemnosc})" + (isFull ? " - [ZAPEŁNIONY]" : ""),
                    Selected = (m.Id == selectedId)
                };

                // Trick dla ASP.NET MVC do wyłączenia opcji w HTML Select za pomocą HtmlAttributes (obsługiwane w nowszych wersjach)
                // Jeśli Twoja wersja frameworka nie nakłada automatycznie 'disabled' przez SelectListItem, obsłużymy to w widoku przez JS.
                if (shouldDisable)
                {
                    item.Disabled = true;
                }

                listItems.Add(listItems.Count == 0 ? new SelectListItem { Value = "", Text = "Wybierz lokalizację" } : item);
                listItems.Add(item);
            }

            // Usuwamy ewentualny duplikat pustego rekordu na górze
            var cleanList = listItems.GroupBy(x => x.Value).Select(g => g.First()).ToList();
            ViewBag.MagazynyList = cleanList;
        }

        private void AktualizujZajetoscMagazynu(int magazynId)
        {
            var magazyn = db.Magazyny.Find(magazynId);
            if (magazyn != null)
            {
                // 1 sprzęt = 1 zajętość
                int count = db.Sprzets.Count(s => s.MagazynId == magazynId);
                magazyn.ZajeteMiejsce = count;
            }
        }

        public ActionResult AddEquipment()
        {
            ViewBag.Statuses = new SelectList(EquipmentStatuses);

            PrepareMagazynDropdown();

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

            // 1. Sprawdzamy, czy użytkownik w ogóle COŚ wpisał
            if (string.IsNullOrWhiteSpace(model.NumerSeryjny))
            {
                // Jeśli pole jest całkowicie puste -> GENERUJEMY AUTOMATYCZNIE
                model.NumerSeryjny = $"EQ-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                // Ponieważ sami wygenerowaliśmy poprawny kod, czyścimy błąd o "niepoprawnym formacie" (np. jeśli silnik walidacji się pomylił)
                if (ModelState["NumerSeryjny"] != null)
                {
                    ModelState["NumerSeryjny"].Errors.Clear();
                }
            }
            else
            {
                // 2. Jeśli użytkownik COŚ wpisał (np. "E"), pozwalamy działać adnotacji [RegularExpression] z modelu Sprzet.cs.
                // Dodatkowo sprawdzamy unikalność tylko wtedy, gdy format jest poprawny:
                if (ModelState["NumerSeryjny"] == null || ModelState["NumerSeryjny"].Errors.Count == 0)
                {
                    bool czyZajety = db.Sprzets.Any(s => s.NumerSeryjny == model.NumerSeryjny);
                    if (czyZajety)
                    {
                        ModelState.AddModelError("NumerSeryjny", "Ten numer seryjny jest już przypisany do innego sprzętu!");
                    }
                }
            }

            // 3. Sprawdzamy ogólny stan walidacji (jeśli wpisano "E", ModelState.IsValid będzie miało wartość false)
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (model.MagazynId.HasValue)
                        {
                            var mag = db.Magazyny.Find(model.MagazynId.Value);
                            if (mag != null && (mag.Status == "Zapełniony" || mag.ZajeteMiejsce >= mag.Pojemnosc))
                            {
                                ModelState.AddModelError("MagazynId", "Wybrany magazyn jest aktualnie przepełniony!");
                                PrepareMagazynDropdown(model.MagazynId);
                                ViewBag.Statuses = new SelectList(EquipmentStatuses, model.Status);
                                return View(model);
                            }
                        }

                        model.CreatedDate = DateTime.Now;
                        db.Sprzets.Add(model);
                        db.SaveChanges(); // Teraz ta linijka jest bezpieczna, bo błędny format ("E") tu nie dotrze

                        if (model.MagazynId.HasValue)
                        {
                            AktualizujZajetoscMagazynu(model.MagazynId.Value);
                            db.SaveChanges();
                        }

                        transaction.Commit();
                        return RedirectToAction("Equipment");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Błąd podczas zapisywania sprzętu: " + ex.Message);
                    }
                }
            }

            // Jeśli walidacja nie przeszła (np. przez wpisanie "E"), odnawiamy listy i zwracamy widok z czerwonym błędem pod polem tekstowym
            PrepareMagazynDropdown(model.MagazynId);
            ViewBag.Statuses = new SelectList(EquipmentStatuses, model.Status);
            return View(model);
        }

        // GET: Equipment/EditEquipment/5
        public ActionResult EditEquipment(int? id) // Zmieniamy int na int? (nullable)
        {
            // Jeśli id nie zostało przesłane w adresie URL, zwracamy błąd 400 (Złe żądanie)
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Brak identyfikatora sprzętu.");
            }

            var sprzet = db.Sprzets.Find(id.Value);
            if (sprzet == null)
            {
                return HttpNotFound();
            }

            ViewBag.Statuses = new SelectList(EquipmentStatuses, sprzet.Status);

            // Załadowanie listy magazynów (wspominane w poprzednich krokach)
            PrepareMagazynDropdown(sprzet.MagazynId);

            return View(sprzet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEquipment(Sprzet model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var originalEquipment = db.Sprzets.Find(model.Id);
                        if (originalEquipment == null) return HttpNotFound();

                        // BLOKADA ZMIANY NUMERU SERYJNEGO:
                        // Jeśli numer wysłany z formularza różni się od tego, który jest zapisany w bazie
                        if (model.NumerSeryjny != originalEquipment.NumerSeryjny)
                        {
                            ModelState.AddModelError("NumerSeryjny", "Nie można zmienić numeru seryjnego po jego utworzeniu!");

                            // Przywracamy oryginalny numer do modelu, aby pole na widoku się zaktualizowało
                            model.NumerSeryjny = originalEquipment.NumerSeryjny;

                            PrepareMagazynDropdown(model.MagazynId);
                            ViewBag.Statuses = new SelectList(EquipmentStatuses, model.Status);
                            return View(model);
                        }

                        int? staryMagazynId = originalEquipment.MagazynId;
                        int? nowyMagazynId = model.MagazynId;

                        if (nowyMagazynId.HasValue && nowyMagazynId != staryMagazynId)
                        {
                            var magNowy = db.Magazyny.Find(nowyMagazynId.Value);
                            if (magNowy != null && (magNowy.Status == "Zapełniony" || magNowy.ZajeteMiejsce >= magNowy.Pojemnosc))
                            {
                                ModelState.AddModelError("MagazynId", "Magazyn docelowy jest przepełniony! Nie można tam przenieść sprzętu.");
                                PrepareMagazynDropdown(model.MagazynId);
                                ViewBag.Statuses = new SelectList(EquipmentStatuses, model.Status);
                                return View(model);
                            }
                        }

                        // Przypisujemy dozwolone do edycji pola
                        originalEquipment.Nazwa = model.Nazwa;
                        originalEquipment.Kategoria = model.Kategoria;
                        originalEquipment.Cena_wynajmu = model.Cena_wynajmu;
                        originalEquipment.Status = model.Status;
                        originalEquipment.MagazynId = model.MagazynId;

                        // UWAGA: Celowo NIE przypisujemy originalEquipment.NumerSeryjny = model.NumerSeryjny;
                        // Dzięki temu wartość w bazie pozostanie nienaruszona.

                        db.SaveChanges();

                        if (staryMagazynId.HasValue) AktualizujZajetoscMagazynu(staryMagazynId.Value);
                        if (nowyMagazynId.HasValue) AktualizujZajetoscMagazynu(nowyMagazynId.Value);

                        db.SaveChanges();

                        transaction.Commit();
                        return RedirectToAction("Equipment");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Błąd podczas modyfikacji danych sprzętu: " + ex.Message);
                    }
                }
            }

            PrepareMagazynDropdown(model.MagazynId);
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

            // KLUCZOWE: Zapamiętujemy ID magazynu w zmiennej tymczasowej,
            // ponieważ po usunięciu obiektu 'sprzet' stracimy dostęp do tej właściwości!
            int? powiazanyMagazynId = sprzet.MagazynId;

            if (sprzet.Status == "Archiwalny")
            {
                try
                {
                    var powiazaneSerwisy = db.Serwisy.Where(s => s.SprzetId == id);
                    db.Serwisy.RemoveRange(powiazaneSerwisy);

                    var powiazaneWynajmy = db.Wynajmy.Where(w => w.SprzetId == id);
                    db.Wynajmy.RemoveRange(powiazaneWynajmy);

                    db.Sprzets.Remove(sprzet);
                    db.SaveChanges(); // Krok 1: Zapisujemy usunięcie zarchiwizowanego sprzętu

                    // Krok 2: Aktualizacja zajętości po usunięciu zarchiwizowanego sprzętu
                    if (powiazanyMagazynId.HasValue)
                    {
                        AktualizujZajetoscMagazynu(powiazanyMagazynId.Value);
                        db.SaveChanges(); // Wyzwala trigger SQL na dbo.Magazyns
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Błąd podczas usuwania zarchiwizowanego sprzętu: " + ex.Message });
                }
            }

            bool maWynajmy = db.Wynajmy.Any(w => w.SprzetId == id) || sprzet.Status == "Wynajęty";
            if (maWynajmy)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie można usunąć tego sprzętu, ponieważ posiada aktywne wynajmy w bazie danych!"
                });
            }

            bool jestAktualnieWSerwisie = db.Serwisy.Any(s => s.SprzetId == id && s.DataZakonczenia == null) || sprzet.Status == "Serwis";
            if (jestAktualnieWSerwisie)
            {
                return Json(new
                {
                    success = false,
                    message = "Nie można usunąć tego sprzętu, ponieważ aktualnie przebywa on w serwisie!"
                });
            }

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
                db.SaveChanges(); // Krok 1: Zapisujemy usunięcie zwykłego sprzętu

                // Krok 2: Aktualizacja zajętości po usunięciu zwykłego sprzętu
                if (powiazanyMagazynId.HasValue)
                {
                    AktualizujZajetoscMagazynu(powiazanyMagazynId.Value);
                    db.SaveChanges(); // Wyzwala trigger SQL na dbo.Magazyns i aktualizuje status
                }

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