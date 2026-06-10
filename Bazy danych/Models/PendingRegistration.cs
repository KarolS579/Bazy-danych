using System;
using System.ComponentModel.DataAnnotations;

namespace Bazy_danych.Models
{
    public class PendingRegistration
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string ConfirmationToken { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
