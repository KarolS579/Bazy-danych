using System;
using System.ComponentModel.DataAnnotations;

namespace Bazy_danych.Models
{
    public class Magazyn
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nazwa magazynu")]
        public string Nazwa { get; set; }

        [Required]
        [Display(Name = "Lokalizacja")]
        public string Lokalizacja { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Pojemność")]
        public int Pojemnosc { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Zajęte miejsce")]
        public int ZajeteMiejsce { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
