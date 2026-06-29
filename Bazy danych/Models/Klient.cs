using System;
using System.ComponentModel.DataAnnotations;

namespace Bazy_danych.Models
{
    public class Klient
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [StringLength(30, ErrorMessage = "Wprowadzona wartość jest zbyt długa (maksymalnie 30 znaków)!")]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s]+$", ErrorMessage = "Imię może zawierać wyłącznie litery.")]
        [Display(Name = "Imię")]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [StringLength(30, ErrorMessage = "Wprowadzona wartość jest zbyt długa (maksymalnie 30 znaków)!")]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ\s\-]+$", ErrorMessage = "Nazwisko może zawierać wyłącznie litery i myślniki.")]
        [Display(Name = "Nazwisko")]
        public string Nazwisko { get; set; }

        [StringLength(30, ErrorMessage = "Wprowadzony numer telefonu jest zbyt długi (maksymalnie 30 znaków)!")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; }

        [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
        [StringLength(50, ErrorMessage = "Wprowadzony e-mail jest zbyt długi (maksymalnie 50 znaków)!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(30, ErrorMessage = "Wprowadzony adres jest zbyt długi (maksymalnie 30 znaków)!")]
        [Display(Name = "Adres")]
        public string Adres { get; set; }

        [StringLength(30, ErrorMessage = "Wprowadzona nazwa firmy jest zbyt długa (maksymalnie 30 znaków)!")]
        [Display(Name = "Firma")]
        public string Firma { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(100, ErrorMessage = "Wprowadzone uwagi są zbyt długie (maksymalnie 100 znaków)!")]
        [Display(Name = "Uwagi")]
        public string Uwagi { get; set; }

        [Display(Name = "Identyfikator Użytkownika")]
        public string ApplicationUserId { get; set; }
    }
}