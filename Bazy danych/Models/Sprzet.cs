using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace Bazy_danych.Models
{
    [Table("Sprzets")] 
    public class Sprzet
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Display(Name = "Numer seryjny / Kod generacji")]
        [RegularExpression(@"^EQ-\d{8}-[A-Z0-9]{4}$",
                    ErrorMessage = "Wprowadzony numer seryjny ma niepoprawną strukturę! Prawidłowy format to EQ-RRRRMMDD-XXXX \n np. EQ-20260627-A1B2")]
        public string NumerSeryjny { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Nazwa")]
        public string Nazwa { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Kategoria")]
        public string Kategoria { get; set; }

        [Display(Name = "Lokalizacja (Magazyn)")]
        public int? MagazynId { get; set; }

        [ForeignKey("MagazynId")]
        public virtual Magazyn Magazyn { get; set; }

        [Required]
        [Range(0.01, 9999)]
        [Display(Name = "Cena wynajmu")]
        public decimal Cena_wynajmu { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } 

        public DateTime CreatedDate { get; set; }
        public virtual ICollection<Wynajem> Wynajmy { get; set; }
    }
}