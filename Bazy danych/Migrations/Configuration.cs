using System.Data.Entity;
using System.Data.Entity.Migrations;
using Bazy_danych.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Bazy_danych.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Bazy_danych.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "Bazy_danych.Models.ApplicationDbContext";
        }

        protected override void Seed(Bazy_danych.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }
        }
    }
}
