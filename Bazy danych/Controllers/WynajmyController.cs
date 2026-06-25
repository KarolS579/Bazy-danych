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

        // LISTA WYNAJMÓW (Index)
        public ActionResult Index()
        {
            // .Include automatycznie dołącza powiązane dane klienta i sprzętu w jednym zapytaniu SQL
            var wynajmy = db.Wynajmy.Include(w => w.Klient).Include(w => w.Sprzets).ToList();
            return View(wynajmy);
        }

        // FORMULARZ DODAWANIA (GET)
        public ActionResult Create()
        {
            db.Configuration.ProxyCreationEnabled = false;

            // IndexOf zwraca -1, jeśli tekst NIE zostanie znaleziony. 
            // Warunek < 0 oznacza dokładnie: "ten status NIE zawiera słowa Wynaj ani Serwis"
            var dostepneSprzety = db.Sprzets
                                    .AsNoTracking()
                                    .ToList()
                                    .Where(s => s.Status == null ||
                                              (s.Status.IndexOf("Wynaj", StringComparison.OrdinalIgnoreCase) < 0 &&
                                               s.Status.IndexOf("Serwis", StringComparison.OrdinalIgnoreCase) < 0))
                                    .ToList();

            ViewBag.SprzetId = new SelectList(dostepneSprzety, "Id", "Nazwa");

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

            // W przypadku błędu walidacji – identyczna, poprawiona logika z < 0
            var dostepneSprzety = db.Sprzets
                                    .AsNoTracking()
                                    .ToList()
                                    .Where(s => s.Status == null ||
                                              (s.Status.IndexOf("Wynaj", StringComparison.OrdinalIgnoreCase) < 0 &&
                                               s.Status.IndexOf("Serwis", StringComparison.OrdinalIgnoreCase) < 0))
                                    .ToList();

            ViewBag.SprzetId = new SelectList(dostepneSprzety, "Id", "Nazwa", wynajem.SprzetId);

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

            // Pobieranie aktualnej daty i godziny (zgodnie z poprzednią zmianą archiwizacji do teraz)
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