using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bazy_danych.Models
{
    public class Sprzet
    {
        public int Id { get; set; }
        public string Nazwa { get; set; }
        public string Kategoria { get; set; }
        public decimal Cena_wynajmu { get; set; }
        public string Status { get; set; } // Dostępny, Wynajęty, Serwis [cite: 30, 45]
    }
}