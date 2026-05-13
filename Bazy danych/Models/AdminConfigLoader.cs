using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bazy_danych.Models
{
    public class AdminUserConfig
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AdminConfigLoader
    {
        /// <summary>
        /// Loads admin users from the AdminUsers.json file in App_Data folder
        /// </summary>
        public static List<AdminUserConfig> LoadAdmins()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "AdminUsers.json");

                if (!File.Exists(jsonPath))
                {
                    return new List<AdminUserConfig>();
                }

                string jsonContent = File.ReadAllText(jsonPath);
                JObject jsonObject = JObject.Parse(jsonContent);

                var admins = new List<AdminUserConfig>();
                if (jsonObject["admins"] is JArray adminsArray)
                {
                    foreach (var adminObj in adminsArray)
                    {
                        admins.Add(new AdminUserConfig
                        {
                            Email = adminObj["email"]?.ToString(),
                            Password = adminObj["password"]?.ToString()
                        });
                    }
                }

                return admins;
            }
            catch (Exception ex)
            {
                // Log error if needed, return empty list
                System.Diagnostics.Debug.WriteLine($"Error loading admin users: {ex.Message}");
                return new List<AdminUserConfig>();
            }
        }
    }
}
