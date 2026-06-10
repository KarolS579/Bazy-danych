using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Bazy_danych.Models
{
    public class ApplicationUser : IdentityUser
    {
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // --- TUTAJ REJESTRUJEMY WSZYSTKIE TABELE ---
        public DbSet<Klient> Klients { get; set; }
        public DbSet<Wynajem> Wynajmy { get; set; }
        public DbSet<Sprzet> Sprzets { get; set; } 
    }
}