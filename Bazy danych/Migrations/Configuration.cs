using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Bazy_danych.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Bazy_danych.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<Bazy_danych.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Bazy_danych.Models.ApplicationDbContext";
        }

        protected override void Seed(Bazy_danych.Models.ApplicationDbContext context)
        {
            // Initialize the user and role managers
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            // 1. Create the Admin and User roles if they don't exist
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }

            // 2. Load admin users from AdminUsers.json configuration file
            var adminsFromConfig = AdminConfigLoader.LoadAdmins();

            // 3. Create admin users from the configuration file
            foreach (var adminConfig in adminsFromConfig)
            {
                if (string.IsNullOrWhiteSpace(adminConfig.Email) || string.IsNullOrWhiteSpace(adminConfig.Password))
                {
                    continue; // Skip if email or password is empty
                }

                var user = userManager.FindByEmail(adminConfig.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = adminConfig.Email,
                        Email = adminConfig.Email
                    };

                    var result = userManager.Create(user, adminConfig.Password);

                    // Assign the user to the Admin role
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }
                else
                {
                    // If user exists but isn't in the Admin role, add them
                    if (!userManager.IsInRole(user.Id, "Admin"))
                    {
                        userManager.AddToRole(user.Id, "Admin");
                    }
                }
            }
        }
    }
}
