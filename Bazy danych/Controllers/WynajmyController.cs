using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize] // Dostęp tylko dla zalogowanych pracowników/adminów
    public class WynajmyController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // 1. LISTA WYNAJMÓW (Index)
        public ActionResult Index()
        {
            // .Include automatycznie dołącza powiązane dane klienta i sprzętu w jednym zapytaniu SQL
            var wynajmy = db.Wynajmy.Include(w => w.Klient).Include(w => w.Sprzets).ToList();
            return View(wynajmy);
        }

        // 2. FORMULARZ DODAWANIA (GET)
        public ActionResult Create()
        {
            // Czyścimy pamięć podręczną kontekstu, aby na pewno pobrać nowy sprzęt po reinstalacji w bazie
            db.Configuration.ProxyCreationEnabled = false;

            // Pobieramy tylko te sprzęty, które NIE są aktualnie wynajęte ani w serwisie
            var dostepneSprzety = db.Sprzets
                                    .AsNoTracking() // Wymuszamy bezpośrednie zapytanie do bazy danych (omijamy Cache)
                                    .Where(s => s.Status == "Dostępny")
                                    .ToList();

            ViewBag.SprzetId = new SelectList(dostepneSprzety, "Id", "Nazwa");

            var listaKlientow = db.Klienci.AsNoTracking().ToList().Select(k => new {
                Id = k.Id,
                PelneDane = k.Imie + " " + k.Nazwisko
            });
            ViewBag.KlientId = new SelectList(listaKlientow, "Id", "PelneDane");

            return View();
        }

        // 3. ZAPIS WYNAJMU (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,DataWynajmu,DataZwrotu,SprzetId,KlientId")] Wynajem wynajem)
        {
            if (ModelState.IsValid)
            {
                // Wszystkie if-y sprawdzające sprzęt i modyfikujące EntityState.Modified zostały usunięte.
                // Trigger w bazie danych wykona to automatycznie po wywołaniu SaveChanges().
                db.Wynajmy.Add(wynajem);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Kod w przypadku błędu walidacji
            ViewBag.SprzetId = new SelectList(db.Sprzets.Where(s => s.Status != "Wynajęte"), "Id", "Nazwa", wynajem.SprzetId);
            var listaKlientow = db.Klienci.ToList().Select(k => new { Id = k.Id, PelneDane = k.Imie + " " + k.Nazwisko });
            ViewBag.KlientId = new SelectList(listaKlientow, "Id", "PelneDane", wynajem.KlientId);
            return View(wynajem);
        }

        // 4. USUWANIE WYNAJMU (POST)
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

        // 5. WYWOŁANIE I AKTUALIZACJA PROCEDURY ARCHIWIZACJI (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CzyscHistorie()
        {
            db.Database.ExecuteSqlCommand(@"
                CREATE OR ALTER PROCEDURE dbo.ZarchiwizujStareWynajmy
                    @DataGraniczna DATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    BEGIN TRANSACTION;
                    BEGIN TRY

                        -- KROK A: Kopiujemy dane ze sprawdzonej tabeli operacyjnej do archiwum
                        INSERT INTO dbo.ArchiwumWynajmow (Id, DataWynajmu, DataZwrotu, SprzetId, KlientId)
                        SELECT Id, DataWynajmu, DataZwrotu, SprzetId, KlientId
                        FROM dbo.Wynajems
                        WHERE DataZwrotu IS NOT NULL AND DataZwrotu <= @DataGraniczna;

                        -- KROK B: Usuwamy rekordy z głównej tabeli
                        DELETE FROM dbo.Wynajems
                        WHERE DataZwrotu IS NOT NULL AND DataZwrotu <= @DataGraniczna;

                        COMMIT TRANSACTION;
                    END TRY
                    BEGIN CATCH
                        -- Awaryjne wycofanie otwartej transakcji w przypadku błędu
                        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                        
                        -- Przekazanie błędu do aplikacji
                        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
                        RAISERROR(@ErrMsg, 16, 1);
                    END CATCH
                END
            ");

            DateTime rokTemu = DateTime.Now.AddDays(-365);

            db.Database.ExecuteSqlCommand(
                "EXEC dbo.ZarchiwizujStareWynajmy @DataGraniczna",
                new System.Data.SqlClient.SqlParameter("@DataGraniczna", rokTemu)
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