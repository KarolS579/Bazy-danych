using System;
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Bazy_danych.Models
{
    // 1. ApplicationUser inherits from IdentityUser, which has built-in Email, PasswordHash, etc.
    public class ApplicationUser : IdentityUser
    {
        // You can add extra custom properties here later (e.g. FirstName, LastName)
    }

    // 2. Inherit from IdentityDbContext to get all the built-in security tables
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext() : base("DefaultConnection", throwIfV1Schema: false) 
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Sprzet> Sprzet { get; set; }
    }
}