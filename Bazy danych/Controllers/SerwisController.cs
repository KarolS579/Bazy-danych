using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    [Authorize]
    public class SerwisController : Controller
    {
        // Korzystamy z tej samej listy sprzętów, która była w Wynajmach
        private static List<Sprzet> sztucznySprzet = new List<Sprzet>()
        {
            new Sprzet { Id = 1, Nazwa = "Wiertarka udarowa Bosch" },
            new Sprzet { Id = 2, Nazwa = "Zagęszczarka do gruntu" }
        };

        // Statyczna lista zgłoszeń serwisowych działająca "na sucho"
        private static List<Serwis> makietaSerwisu = new List<Serwis>()
        {
            new Serwis
            {
                Id = 1,
                OpisUsterki = "Wymiana szczotek węglowych i kabla zasilającego",
                DataZgloszenia = DateTime.Now.AddDays(-4),
                Koszt = 120.00m,
                Status = "Naprawiono",
                SprzetId = 1,
                Sprzet = sztucznySprzet[0]
            },
            new Serwis
            {
                Id = 2,
                OpisUsterki = "Silnik gaśnie pod obciążeniem - czyszczenie gaźnika",
                DataZgloszenia = DateTime.Now,
                Koszt = null,
                Status = "W diagnozie",
                SprzetId = 2,
                Sprzet = sztucznySprzet[1]
            }
        };

        public ActionResult Index()
        {
            return View(makietaSerwisu);
        }

        public ActionResult Create()
        {
            ViewBag.SprzetId = new SelectList(sztucznySprzet, "Id", "Nazwa");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Serwis serwis)
        {
            if (ModelState.IsValid)
            {
                serwis.Id = makietaSerwisu.Count > 0 ? makietaSerwisu.Max(s => s.Id) + 1 : 1;
                serwis.DataZgloszenia = DateTime.Now;
                serwis.Status = "W diagnozie";

                serwis.Sprzet = sztucznySprzet.FirstOrDefault(s => s.Id == serwis.SprzetId);

                makietaSerwisu.Add(serwis);
                return RedirectToAction("Index");
            }

            ViewBag.SprzetId = new SelectList(sztucznySprzet, "Id", "Nazwa", serwis.SprzetId);
            return View(serwis);
        }
    }
}