using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class UserEquipmentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // 1. LISTA DOSTĘPNEGO SPRZĘTU
        public ActionResult Index()
        {
            // Prywatność: Pobieramy tylko i wyłącznie sprzęt ze statusem "dostępny"
            var dostepnySprzet = db.Sprzets
                .Include(s => s.Magazyn)
                .ToList() // Pobranie do pamięci, aby bezpiecznie oczyścić stringi statusów
                .Where(s => (s.Status ?? "").Trim().ToLower() == "dostępny" ||
                            (s.Status ?? "").Trim().ToLower() == "dostepny")
                .ToList();

            return View(dostepnySprzet);
        }

        // 2. SFORMALIZOWANA STRONA SKLEPOWA (SZCZEGÓŁY)
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var sprzet = db.Sprzets.Include(s => s.Magazyn).FirstOrDefault(s => s.Id == id);

            // Blokada: Jeśli sprzęt nie istnieje lub nie jest dostępny, ukrywamy go przed klientem
            if (sprzet == null) return HttpNotFound();

            var status = (sprzet.Status ?? "").Trim().ToLower();
            if (status != "dostępny" && status != "dostepny")
            {
                return HttpNotFound();
            }

            return View(sprzet);
        }
    }
}