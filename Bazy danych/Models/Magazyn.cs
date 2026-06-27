using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bazy_danych.Models
{
    [Table("Magazyns")]
    public class Magazyn
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Nazwa magazynu")]
        public string Nazwa { get; set; }

        [Required]
        [MaxLength(20)]
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
