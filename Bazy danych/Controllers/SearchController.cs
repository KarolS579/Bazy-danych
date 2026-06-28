using Bazy_danych.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;

namespace Bazy_danych.Controllers
{
    [Authorize]
    [OutputCache(NoStore = true, Duration = 0, Location = OutputCacheLocation.None)]
    public class SearchController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Search/Results
        [HttpGet]
        public ActionResult Results(string query = "", string typ = "Wszystko")
        {
            // ZABEZPIECZENIE: Pobranie listy z sesji zapobiega rzucaniu błędu NullReferenceException w widoku
            var listaIds = Session["Porownywarka"] as List<int> ?? new List<int>();

            var viewModel = new GlobalSearchViewModel
            {
                SearchQuery = query,
                WybranyTyp = string.IsNullOrEmpty(typ) ? "Wszystko" : typ,
                Sortowanie = "NazwaAsc",
                PorownywarkaIds = listaIds,
                ZnalezionySprzet = new List<Sprzet>(),
                ZnalezieniKlienci = new List<Klient>(),
                ZnalezioneWynajmy = new List<Wynajem>(),
                ZnalezioneSerwisy = new List<Serwis>(),
                ZnalezioneMagazyny = new List<Magazyn>()
            };

            return View(viewModel);
        }

        // GET: Search/GetAutocompleteSuggestions
        [HttpGet]
        public JsonResult GetAutocompleteSuggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }

            string t = term.Trim().ToLower();

            // 1. SPRZĘT - GroupBy usuwa dokładne kopie nazw maszyn
            var sprzetList = db.Sprzets
                .Where(s => s.Nazwa.ToLower().Contains(t) || s.NumerSeryjny.ToLower().Contains(t))
                .GroupBy(s => s.Nazwa)
                .Select(g => new { Tekst = g.Key, Typ = "Sprzet" })
                .Take(3)
                .ToList();

            // 2. MAGAZYNY - GroupBy usuwa powtórzenia
            var magazynList = db.Magazyny
                .Where(m => m.Nazwa.ToLower().Contains(t) || m.Lokalizacja.ToLower().Contains(t))
                .GroupBy(m => m.Nazwa)
                .Select(g => new { Tekst = g.Key, Typ = "Magazyny" })
                .Take(3)
                .ToList();

            // 3. KLIENCI - Łączymy Imię, Nazwisko oraz Firmę w jeden czytelny ciąg tekstowy
            var klienciList = db.Klienci
                .Where(k => k.Imie.ToLower().Contains(t) || k.Nazwisko.ToLower().Contains(t) || (k.Firma != null && k.Firma.ToLower().Contains(t)))
                .Select(k => new
                {
                    // Format: "Imię Nazwisko (Nazwa Firmy)" lub samo "Imię Nazwisko" jeśli brak firmy
                    Tekst = k.Imie + " " + k.Nazwisko + (string.IsNullOrEmpty(k.Firma) ? "" : " (" + k.Firma + ")")
                })
                .GroupBy(n => n.Tekst) // Usuwanie dokładnych kopii (duplikatów)
                .Select(g => new { Tekst = g.Key, Typ = "Klienci" })
                .Take(3)
                .ToList();

            // 4. SERWIS - Przeszukiwanie pola 'Opis' (zgodnie z Serwis.cs) i grupowanie
            var serwisList = db.Serwisy
                .Where(s => s.Opis.ToLower().Contains(t))
                .GroupBy(s => s.Opis)
                .Select(g => new { Tekst = g.Key, Typ = "Serwis" })
                .Take(3)
                .ToList();

            // 5. WYNAJMY - Wyszukiwanie na podstawie powiązanego sprzętu lub danych klienta
            var wynajemList = db.Wynajmy
                .Include(w => w.Sprzets)
                .Include(w => w.Klient)
                .Where(w => w.Sprzets.Nazwa.ToLower().Contains(t) || w.Klient.Nazwisko.ToLower().Contains(t))
                .Select(w => w.Sprzets.Nazwa + " -> " + w.Klient.Imie + " " + w.Klient.Nazwisko)
                .GroupBy(wText => wText)
                .Select(g => new { Tekst = g.Key, Typ = "Wynajmy" })
                .Take(3)
                .ToList();

            // Scalenie wyników z zachowaniem unikalności
            var combined = sprzetList.Cast<object>()
                .Concat(magazynList)
                .Concat(klienciList)
                .Concat(serwisList)
                .Concat(wynajemList)
                .ToList();

            return Json(combined, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult LiveSearch(string query, string typ, string sort,
                        decimal? cenaOd, decimal? cenaDo, string numSeryjny, string kategoria,
                        string lokalizacja, int? pojemnoscMin, string magazynStatus, DateTime? magazynDataOd, DateTime? magazynDataDo)
        {
            var viewModel = new GlobalSearchViewModel
            {
                SearchQuery = query,
                WybranyTyp = string.IsNullOrEmpty(typ) ? "Wszystko" : typ,
                Sortowanie = string.IsNullOrEmpty(sort) ? "NazwaAsc" : sort,
                ZnalezionySprzet = new List<Sprzet>(),
                ZnalezieniKlienci = new List<Klient>(),
                ZnalezioneWynajmy = new List<Wynajem>(),
                ZnalezioneSerwisy = new List<Serwis>(),
                ZnalezioneMagazyny = new List<Magazyn>(),
                PorownywarkaIds = Session["Porownywarka"] as List<int> ?? new List<int>()
            };

            bool czyJestFraza = !string.IsNullOrWhiteSpace(query);
            string sOrder = viewModel.Sortowanie;

            // TYLKO JEDNA, POPRAWNA DEKLARACJA ZMIENNEJ 'words' Z OCZYSZCZANIEM ZNAKÓW SPECIALNYCH:
            string[] words = new string[0];
            if (czyJestFraza)
            {
                // Zamieniamy znaki dekoracyjne z Autocomplete na spacje
                string cleanedQuery = query
                    .Replace("->", " ")
                    .Replace("(", " ")
                    .Replace(")", " ")
                    .Replace("-", " ")
                    .Replace(",", " ");

                words = cleanedQuery.Trim().ToLower()
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            // 1. SPRZĘT
            if (viewModel.WybranyTyp == "Wszystko" || viewModel.WybranyTyp == "Sprzet")
            {
                var q = db.Sprzets.Include(s => s.Magazyn).AsQueryable();

                foreach (var word in words)
                {
                    string w = word;
                    q = q.Where(s => s.Nazwa.ToLower().Contains(w) ||
                                     s.Kategoria.ToLower().Contains(w) ||
                                     s.Status.ToLower().Contains(w) ||
                                     s.NumerSeryjny.ToLower().Contains(w) ||
                                     (s.Magazyn != null && s.Magazyn.Nazwa.ToLower().Contains(w)));
                }

                if (cenaOd.HasValue) q = q.Where(s => s.Cena_wynajmu >= cenaOd.Value);
                if (cenaDo.HasValue) q = q.Where(s => s.Cena_wynajmu <= cenaDo.Value);
                if (!string.IsNullOrEmpty(numSeryjny)) q = q.Where(s => s.NumerSeryjny.Contains(numSeryjny));
                if (!string.IsNullOrEmpty(kategoria)) q = q.Where(s => s.Kategoria.Contains(kategoria));

                switch (sOrder)
                {
                    case "NazwaDesc": q = q.OrderByDescending(s => s.Nazwa); break;
                    case "CenaAsc": q = q.OrderBy(s => s.Cena_wynajmu); break;
                    case "CenaDesc": q = q.OrderByDescending(s => s.Cena_wynajmu); break;
                    case "KategoriaAsc": q = q.OrderBy(s => s.Kategoria); break;
                    case "StatusAsc": q = q.OrderBy(s => s.Status); break;
                    case "DataNajnowsze": q = q.OrderByDescending(s => s.CreatedDate); break;
                    default: q = q.OrderBy(s => s.Nazwa); break;
                }

                viewModel.ZnalezionySprzet = q.Take(15).ToList();
            }

            // 2. MAGAZYNY (Skonsolidowane filtry podstawowe i zaawansowane)
            if (viewModel.WybranyTyp == "Wszystko" || viewModel.WybranyTyp == "Magazyny")
            {
                var qMagazyny = db.Magazyny.AsQueryable();

                if (czyJestFraza)
                {
                    int liczbaWyszukiwana;
                    if (int.TryParse(query, out liczbaWyszukiwana))
                    {
                        qMagazyny = qMagazyny.Where(m => m.Pojemnosc >= liczbaWyszukiwana);
                    }
                    else
                    {
                        qMagazyny = qMagazyny.Where(m => m.Nazwa.ToLower().Contains(query) || m.Lokalizacja.ToLower().Contains(query));
                    }
                }

                if (!string.IsNullOrEmpty(lokalizacja))
                {
                    string lok = lokalizacja.Trim().ToLower();
                    qMagazyny = qMagazyny.Where(m => m.Lokalizacja.ToLower().Contains(lok));
                }

                if (pojemnoscMin.HasValue)
                {
                    qMagazyny = qMagazyny.Where(m => m.Pojemnosc >= pojemnoscMin.Value);
                }

                if (!string.IsNullOrEmpty(magazynStatus))
                {
                    if (magazynStatus == "Zapełniony")
                    {
                        // A warehouse is 'Zapełniony' if occupied space is greater or equal to its capacity
                        qMagazyny = qMagazyny.Where(m => m.ZajeteMiejsce >= m.Pojemnosc);
                    }
                    else if (magazynStatus == "Aktywny")
                    {
                        // A warehouse is 'Aktywny' if it has positive capacity and available space
                        qMagazyny = qMagazyny.Where(m => m.Pojemnosc > 0 && m.ZajeteMiejsce < m.Pojemnosc);
                    }
                    // No 'else' needed for 'Wszystkie statusy' as the filter is only applied if magazynStatus is not empty.
                }

                if (magazynDataOd.HasValue) qMagazyny = qMagazyny.Where(m => m.CreatedDate >= magazynDataOd.Value);
                if (magazynDataDo.HasValue) qMagazyny = qMagazyny.Where(m => m.CreatedDate <= magazynDataDo.Value);

                qMagazyny = (sOrder == "NazwaDesc") ? qMagazyny.OrderByDescending(m => m.Nazwa) : qMagazyny.OrderBy(m => m.Nazwa);
                viewModel.ZnalezioneMagazyny = qMagazyny.Take(15).ToList();
            }

            // 3. KLIENCI
            if (viewModel.WybranyTyp == "Wszystko" || viewModel.WybranyTyp == "Klienci")
            {
                var q = db.Klienci.AsQueryable();

                foreach (var word in words)
                {
                    string w = word;
                    // Szuka w Imieniu, Nazwisku LUB Nazwie firmy
                    q = q.Where(k => k.Imie.ToLower().Contains(w) ||
                                     k.Nazwisko.ToLower().Contains(w) ||
                                     (k.Firma != null && k.Firma.ToLower().Contains(w)));
                }

                if (sOrder == "NazwaDesc") q = q.OrderByDescending(k => k.Nazwisko);
                else q = q.OrderBy(k => k.Nazwisko);

                viewModel.ZnalezieniKlienci = q.Take(15).ToList();
            }

            // 4. WYNAJMY
            if (viewModel.WybranyTyp == "Wszystko" || viewModel.WybranyTyp == "Wynajmy")
            {
                var q = db.Wynajmy.Include(w => w.Sprzets).Include(w => w.Klient).AsQueryable();

                foreach (var word in words)
                {
                    string w = word;
                    q = q.Where(wItem => wItem.Sprzets.Nazwa.ToLower().Contains(w) ||
                                         wItem.Sprzets.NumerSeryjny.ToLower().Contains(w) ||
                                         wItem.Klient.Imie.ToLower().Contains(w) ||
                                         wItem.Klient.Nazwisko.ToLower().Contains(w) ||
                                         (wItem.Klient.Firma != null && wItem.Klient.Firma.ToLower().Contains(w)));
                }

                if (sOrder == "NazwaDesc") q = q.OrderByDescending(wItem => wItem.Sprzets.Nazwa);
                else if (sOrder == "DataNajnowsze") q = q.OrderByDescending(wItem => wItem.DataWynajmu);
                else q = q.OrderBy(wItem => wItem.Sprzets.Nazwa);

                viewModel.ZnalezioneWynajmy = q.Take(15).ToList();
            }

            // 5. SERWIS
            if (viewModel.WybranyTyp == "Wszystko" || viewModel.WybranyTyp == "Serwis")
            {
                var q = db.Serwisy.Include(s => s.Sprzet).AsQueryable();

                foreach (var word in words)
                {
                    string w = word;
                    q = q.Where(sItem => sItem.Opis.ToLower().Contains(w) ||
                                         sItem.Sprzet.Nazwa.ToLower().Contains(w) ||
                                         sItem.Sprzet.NumerSeryjny.ToLower().Contains(w));
                }

                if (sOrder == "NazwaDesc") q = q.OrderByDescending(sItem => sItem.Sprzet.Nazwa);
                else q = q.OrderBy(sItem => sItem.Sprzet.Nazwa);

                viewModel.ZnalezioneSerwisy = q.Take(15).ToList();
            }

            return PartialView("_SearchResults", viewModel);
        }

        [HttpPost]
        public JsonResult DodajDoPorownania(int id)
        {
            var sprzet = db.Sprzets.Find(id);
            if (sprzet == null || sprzet.Status != "Dostępny")
                return Json(new { success = false, message = "Sprzęt jest niedostępny lub nie istnieje." });

            var lista = Session["Porownywarka"] as List<int> ?? new List<int>();

            if (!lista.Contains(id))
            {
                lista.Add(id);
                Session["Porownywarka"] = lista;
            }

            return Json(new { success = true, count = lista.Count });
        }

        [HttpPost]
        public JsonResult UsunZPorownania(int id)
        {
            var lista = Session["Porownywarka"] as List<int> ?? new List<int>();

            if (lista.Contains(id))
            {
                lista.Remove(id);
                Session["Porownywarka"] = lista;
            }

            return Json(new { success = true, count = lista.Count });
        }

        public ActionResult Porownaj()
        {
            var listaIds = Session["Porownywarka"] as List<int> ?? new List<int>();
            var modele = db.Sprzets.Include(s => s.Magazyn).Where(s => listaIds.Contains(s.Id)).ToList();
            return View(modele);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}