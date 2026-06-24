using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bazy_danych.Models
{
    [Table("ArchiwumWynajmow")]
    public class ArchiwumWynajmu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Data wynajmu")]
        [DataType(DataType.Date)]
        public DateTime DataWynajmu { get; set; }

        [Display(Name = "Data zwrotu")]
        [DataType(DataType.Date)]
        public DateTime? DataZwrotu { get; set; }

        [Required]
        [Display(Name = "Sprzęt")]
        public int SprzetId { get; set; }

        [ForeignKey("SprzetId")]
        public virtual Sprzet Sprzets { get; set; }

        [Required]
        [Display(Name = "Klient")]
        public int KlientId { get; set; }

        [ForeignKey("KlientId")]
        public virtual Klient Klient { get; set; }

        [Required]
        [Display(Name = "Zarchiwizowano dnia")]
        public DateTime DataArchiwizacji { get; set; }
    }
}