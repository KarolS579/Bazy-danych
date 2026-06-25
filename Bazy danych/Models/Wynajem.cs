using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bazy_danych.Models
{
    [Table("Wynajems")]
    public class Wynajem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data wynajmu")]
        [DataType(DataType.Date)]
        public DateTime DataWynajmu { get; set; }

        [Display(Name = "Data zwrotu")]
        [DataType(DataType.Date)]
        public DateTime? DataZwrotu { get; set; }

        // POWIĄZANIE ZE SPRZĘTEM
        [Required]
        [Display(Name = "Wybierz sprzęt")]
        public int SprzetId { get; set; }

        [ForeignKey("SprzetId")]
        public virtual Sprzet Sprzets { get; set; } // Pozwala wyciągnąć np. Sprzet.Nazwa

        // POWIĄZANIE Z KLIENTEM
        [Required]
        [Display(Name = "Wybierz klienta")]
        public int KlientId { get; set; }

        [ForeignKey("KlientId")]
        public virtual Klient Klient { get; set; } // Pozwala wyciągnąć np. Klient.Imie i Klient.Nazwisko
    }
}