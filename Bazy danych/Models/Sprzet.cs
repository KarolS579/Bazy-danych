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

        [Required]
        [MaxLength(20)]
        [Display(Name = "Nazwa")]
        public string Nazwa { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Kategoria")]
        public string Kategoria { get; set; }

        [Required]
        [Range(0.01, 9999)]
        [Display(Name = "Cena wynajmu")]
        public decimal Cena_wynajmu { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } 

        public DateTime CreatedDate { get; set; }
    }
}