using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Bazy_danych.Models
{
    // ApplicationUser inherits from IdentityUser, which has built-in Email, PasswordHash, etc.
    public class ApplicationUser : IdentityUser
    {
    }

    // Inherit from IdentityDbContext to get all the built-in security tables
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("name=DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Sprzet> Sprzets { get; set; }
        public DbSet<Magazyn> Magazyny { get; set; }
        public DbSet<Klient> Klienci { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }
        public DbSet<Wynajem> Wynajmy { get; set; }
        public DbSet<Serwis> Serwisy { get; set; }

        public DbSet<ArchiwumWynajmu> ArchiwumWynajmow { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja relacji: Jeden Sprzęt ma Wiele Wynajmów
            modelBuilder.Entity<Wynajem>()
                .HasRequired(w => w.Sprzets)
                .WithMany(s => s.Wynajmy)
                .HasForeignKey(w => w.SprzetId)
                .WillCascadeOnDelete(false); // Blokada No Action / Restrict
        }
    }
}