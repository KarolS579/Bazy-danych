using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Bazy_danych.Models
{
    public class Sprzet
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Nazwa")]
        public string Nazwa { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Kategoria")]
        public string Kategoria { get; set; }

        [Required]
        [Range(0.01, 9999999)]
        [Display(Name = "Cena wynajmu")]
        public decimal Cena_wynajmu { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } // Dostępny, Wynajęty, Serwis [cite: 30, 45]

        public DateTime CreatedDate { get; set; }
    }
}