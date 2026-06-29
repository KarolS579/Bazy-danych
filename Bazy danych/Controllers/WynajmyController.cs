using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Bazy_danych.Models;
using Rotativa; // <--- POTRZEBNE DO GENEROWANIA PDF

namespace Bazy_danych.Controllers
{
    [Authorize] // Dostęp do całego kontrolera tylko dla zalogowanych pracowników/adminów
    public class WynajmyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // LISTA WYNAJMÓW (Index)
        public ActionResult Index()
        {
            // .Include automatycznie dołącza powiązane dane klienta i sprzętu w jednym zapytaniu SQL
            var wynajmy = db.Wynajmy.Include(w => w.Klient).Include(w => w.Sprzets).ToList();
            return View(wynajmy);
        }

        // 1. AKCJA WYWOŁYWANA PRZEZ PRZYCISK - GENERUJE I POBIERA PDF
        public ActionResult DrukujPdf(int id)
        {
            var wynajem = db.Wynajmy
                            .Include(w => w.Klient)
                            .Include(w => w.Sprzets)
                            .FirstOrDefault(w => w.Id == id);

            if (wynajem == null)
            {
                return HttpNotFound();
            }

            // Zwracamy widok 'SzablonPdf' jako gotowy plik PDF do pobrania
            return new ActionAsPdf("SzablonPdf", new { id = id })
            {
                FileName = $"Potwierdzenie_Wynajmu_Nr_{wynajem.Id}.pdf"
            };
        }

        // 2. AKCJA POMOCNICZA - DOSTARCZA DANE DO SZABLONU PDF I LICZY CENĘ
        [AllowAnonymous] // <--- KLUCZOWA ZMIANA: Pozwala Rotativie odczytać dane szablonu bez blokady logowania
        public ActionResult SzablonPdf(int id)
        {
            var wynajem = db.Wynajmy
                            .Include(w => w.Klient)
                            .Include(w => w.Sprzets)
                            .FirstOrDefault(w => w.Id == id);

            if (wynajem == null)
            {
                return HttpNotFound();
            }

            // OBLICZANIE CENY KOŃCOWEJ - Zmiana na Twoje pole: Cena_wynajmu
            decimal cenaZaDzien = wynajem.Sprzets.Cena_wynajmu;

            // Jeśli sprzęt nie został jeszcze zwrócony, liczymy dni od daty wynajmu do dziś
            DateTime dataKoncowa = wynajem.DataZwrotu ?? DateTime.Now;

            // Obliczamy różnicę w dniach (minimum 1 dzień)
            int liczbaDni = (dataKoncowa.Date - wynajem.DataWynajmu.Date).Days;
            if (liczbaDni <= 0) liczbaDni = 1;

            decimal lacznaCena = liczbaDni * cenaZaDzien;

            // Przekazujemy obliczenia do widoku PDF za pomocą ViewBag
            ViewBag.LiczbaDni = liczbaDni;
            ViewBag.LacznaCena = lacznaCena;

            return View(wynajem);
        }

        // FORMULARZ DODAWANIA (GET)
        public ActionResult Create()
        {
            db.Configuration.ProxyCreationEnabled = false;

            // 1. Pobieramy przefiltrowaną lista sprzętów z bazy danych
            var dostepneSprzety = db.Sprzets
                                    .AsNoTracking()
                                    .ToList()
                                    .Where(s => s.Status == null ||
                                              (s.Status.IndexOf("Wynaj", StringComparison.OrdinalIgnoreCase) < 0 &&
                                               s.Status.IndexOf("Serwis", StringComparison.OrdinalIgnoreCase) < 0))
                                    .ToList();

            // 2. ŁĄCZENIE PARAMETRÓW: Mapujemy sprzęty na listę elementów SelectListItem,
            // łącząc NumerSeryjny z Nazwą w formacie: [Numer] Nazwa
            ViewBag.SprzetId = dostepneSprzety.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"[{s.NumerSeryjny}] {s.Nazwa}"
            }).ToList();

            // 3. Przygotowanie listy klientów (bez zmian)
            var listaKlientow = db.Klienci.AsNoTracking().ToList().Select(k => new {
                Id = k.Id,
                PelneDane = k.Imie + " " + k.Nazwisko
            });
            ViewBag.KlientId = new SelectList(listaKlientow, "Id", "PelneDane");

            return View();
        }

        // ZAPIS WYNAJMU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DataWynajmu,DataZwrotu,SprzetId,KlientId")] Wynajem wynajem)
        {
            if (ModelState.IsValid)
            {
                db.Wynajmy.Add(wynajem);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            var dostepneSprzety = db.Sprzets
                                    .AsNoTracking()
                                    .ToList()
                                    .Where(s => s.Status == null ||
                                              (s.Status.IndexOf("Wynaj", StringComparison.OrdinalIgnoreCase) < 0 &&
                                               s.Status.IndexOf("Serwis", StringComparison.OrdinalIgnoreCase) < 0))
                                    .ToList();

            ViewBag.SprzetId = dostepneSprzety.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"[{s.NumerSeryjny}] {s.Nazwa}",
                Selected = (s.Id == wynajem.SprzetId)
            }).ToList();

            var listaKlientow = db.Klienci.AsNoTracking().ToList().Select(k => new { Id = k.Id, PelneDane = k.Imie + " " + k.Nazwisko });
            ViewBag.KlientId = new SelectList(listaKlientow, "Id", "PelneDane", wynajem.KlientId);
            return View(wynajem);
        }

        // USUWANIE WYNAJMU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var wynajem = db.Wynajmy.Find(id);

            if (wynajem != null)
            {
                var sprzet = db.Sprzets.Find(wynajem.SprzetId);
                if (sprzet != null)
                {
                    sprzet.Status = "Dostępny";
                    db.Entry(sprzet).State = EntityState.Modified;
                }

                db.Wynajmy.Remove(wynajem);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CzyscHistorie()
        {
            // Definicja / Aktualizacja procedury
            db.Database.ExecuteSqlCommand(@"
        CREATE OR ALTER PROCEDURE dbo.ZarchiwizujStareWynajmy
            @DataGraniczna DATETIME
        AS
        BEGIN
            SET NOCOUNT ON;
                                        
            BEGIN TRANSACTION;
            BEGIN TRY
                INSERT INTO dbo.ArchiwumWynajmow (Id, DataWynajmu, DataZwrotu, SprzetId, KlientId, DataArchiwizacji)
                SELECT Id, DataWynajmu, DataZwrotu, SprzetId, KlientId, GETDATE()
                FROM dbo.Wynajems
                WHERE DataZwrotu IS NOT NULL AND DataZwrotu < @DataGraniczna;

                UPDATE dbo.Sprzets
                SET Status = 'Dostępny'
                WHERE Id IN (
                    SELECT SprzetId 
                    FROM dbo.Wynajems 
                    WHERE DataZwrotu IS NOT NULL AND DataZwrotu < @DataGraniczna
                );

                DELETE FROM dbo.Wynajems
                WHERE DataZwrotu IS NOT NULL AND DataZwrotu < @DataGraniczna;

                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0 
                BEGIN
                    ROLLBACK TRANSACTION;
                END;

                THROW;
            END CATCH
        END
    ");

            // Pobieranie aktualnej daty i godziny
            DateTime teraz = DateTime.Now.AddDays(-1);

            // Wywołanie procedury
            db.Database.ExecuteSqlCommand(
                "EXEC dbo.ZarchiwizujStareWynajmy @DataGraniczna",
                new System.Data.SqlClient.SqlParameter("@DataGraniczna", teraz)
            );

            return RedirectToAction("Index");
        }

        public ActionResult Archiwum()
        {
            var archiwum = db.ArchiwumWynajmow
                             .Include(a => a.Klient)
                             .Include(a => a.Sprzets)
                             .OrderByDescending(a => a.DataArchiwizacji)
                             .ToList();

            return View(archiwum);
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