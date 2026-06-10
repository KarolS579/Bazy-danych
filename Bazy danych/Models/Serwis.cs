using System;
using System.ComponentModel.DataAnnotations;

namespace Bazy_danych.Models
{
    public class Serwis
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Opis usterki")]
        public string OpisUsterki { get; set; }

        [Required]
        [Display(Name = "Data zgłoszenia")]
        [DataType(DataType.Date)]
        public DateTime DataZgloszenia { get; set; }

        [Display(Name = "Koszt naprawy (zł)")]
        public decimal? Koszt { get; set; }

        [Display(Name = "Status naprawy")]
        public string Status { get; set; } // np. "W diagnozie", "Naprawiono", "Wymaga części"

        // Relacja do sprzętu
        public int SprzetId { get; set; }
        public virtual Sprzet Sprzet { get; set; }
    }
}