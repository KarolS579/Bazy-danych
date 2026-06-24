using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bazy_danych.Models
{
    [Table("Serwises")] 
    public class Serwis
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data rozpoczęcia serwisu")]
        [DataType(DataType.Date)]
        public DateTime DataRozpoczecia { get; set; }

        [Display(Name = "Data zakończenia serwisu")]
        [DataType(DataType.Date)]
        public DateTime? DataZakonczenia { get; set; }

        [MaxLength(100)]
        [Display(Name = "Opis usterki / Uwagi")]
        public string Opis { get; set; }

        [Required]
        [Display(Name = "Sprzęt")]
        public int SprzetId { get; set; }

        [ForeignKey("SprzetId")]
        public virtual Sprzet Sprzet { get; set; }
    }
}