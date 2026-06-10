using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bazy_danych.Models
{
    public class Wynajem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data wypożyczenia")]
        [DataType(DataType.Date)]
        public DateTime DataWypozyczenia { get; set; }

        [Display(Name = "Data zwrotu")]
        [DataType(DataType.Date)]
        public DateTime? DataZwrotu { get; set; } 


        [Display(Name = "Klient")]
        public int KlientId { get; set; }
        [ForeignKey("KlientId")]
        public virtual Klient Klient { get; set; } 

        [Display(Name = "Sprzęt")]
        public int SprzetId { get; set; }
        [ForeignKey("SprzetId")]
        public virtual Sprzet Sprzet { get; set; } 
    }
}