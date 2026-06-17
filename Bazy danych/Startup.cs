using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(Bazy_danych.Startup))]
namespace Bazy_danych
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),

                // 1. Set how long the session remains valid during inactivity (e.g., 15 minutes)
                ExpireTimeSpan = TimeSpan.FromMinutes(15),

                // 2. Resets the 15-minute timer every time the user clicks or reloads a page
                SlidingExpiration = true
            });
        }
    }
}