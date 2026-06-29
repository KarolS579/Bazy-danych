using System;
using System.Collections.Generic;

namespace Bazy_danych.Models
{
    public class GlobalSearchViewModel
    {
        public string SearchQuery { get; set; }
        public string WybranyTyp { get; set; }
        public string Sortowanie { get; set; }

        public List<Sprzet> ZnalezionySprzet { get; set; }
        public List<Klient> ZnalezieniKlienci { get; set; }
        public List<Wynajem> ZnalezioneWynajmy { get; set; }
        public List<Serwis> ZnalezioneSerwisy { get; set; }

        // NOWE: Przechowywanie niezależnych obiektów magazynów w wynikach wyszukiwania
        public List<Magazyn> ZnalezioneMagazyny { get; set; }

        public List<int> PorownywarkaIds { get; set; }
    }
}