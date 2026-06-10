using System.ComponentModel.DataAnnotations;

namespace Bazy_danych.Models
{
    public class Klient
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Imię")]
        public string Imie { get; set; }

        [Required]
        [Display(Name = "Nazwisko")]
        public string Nazwisko { get; set; }

        [Display(Name = "Telefon")]
        public string Telefon { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Adres")]
        public string Adres { get; set; }

        [Display(Name = "Firma")]
        public string Firma { get; set; }

        [Display(Name = "Uwagi")]
        public string Uwagi { get; set; }
    }
}
