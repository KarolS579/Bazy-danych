using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bazy_danych.Models;

namespace Bazy_danych.Controllers
{
    public class KlienciController : Controller
    {
        private static List<Klient> makietaKlientow = new List<Klient>()
        {
            new Klient { Id = 1, Nazwisko = "Jan Kowalski", Email = "jan.kowalski@gmail.com" },
            new Klient { Id = 2, Nazwisko = "Piotr Nowak - Firma", Email = "kontakt@nowak-bud.pl" }
        };

        public ActionResult Index()
        {
            return View(makietaKlientow);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Klient klient)
        {
            if (ModelState.IsValid)
            {
                klient.Id = makietaKlientow.Count > 0 ? makietaKlientow.Max(k => k.Id) + 1 : 1;

                makietaKlientow.Add(klient);

                return RedirectToAction("Index");
            }

            return View(klient);
        }
    }
}