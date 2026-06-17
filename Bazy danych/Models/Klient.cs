using System;
using System.ComponentModel.DataAnnotations;

namespace Bazy_danych.Models
{
    public class Klient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(50, ErrorMessage = "Wprowadzona wartość jest zbyt długa (maksymalnie 50 znaków)!")]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s]+$", ErrorMessage = "Imię może zawierać wyłącznie litery.")]
        [Display(Name = "Imię")]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(50, ErrorMessage = "Wprowadzona wartość jest zbyt długa (maksymalnie 50 znaków)!")]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-]+$", ErrorMessage = "Nazwisko może zawierać wyłącznie litery i myślniki.")]
        [Display(Name = "Nazwisko")]
        public string Nazwisko { get; set; }

        [StringLength(20, ErrorMessage = "Wprowadzony numer telefonu jest zbyt długi (maksymalnie 20 znaków)!")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }

        [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
        [StringLength(100, ErrorMessage = "Wprowadzony e-mail jest zbyt długi (maksymalnie 100 znaków)!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(200, ErrorMessage = "Wprowadzony adres jest zbyt długi (maksymalnie 200 znaków)!")]
        [Display(Name = "Adres")]
        public string Adres { get; set; }

        [StringLength(100, ErrorMessage = "Wprowadzona nazwa firmy jest zbyt długa (maksymalnie 100 znaków)!")]
        [Display(Name = "Firma")]
        public string Firma { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Wprowadzone uwagi są zbyt długie (maksymalnie 1000 znaków)!")]
        [Display(Name = "Uwagi")]
        public string Uwagi { get; set; }
    }
}