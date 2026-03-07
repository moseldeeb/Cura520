using Cura520.DataAccess;
using Cura520.Models;
using Cura520.Utilities;

namespace Cura520
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            
            // Use centralized configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.Config(connectionString);

            var app = builder.Build();

            // Call this before app.Run()
            SeedDatabase();

            void SeedDatabase()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializr>();
                    dbInitializer.Initialize();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // Redirect root URL to login
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/" && !context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Identity/Account/Login");
                    return;
                }
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            // 1. Map for Areas (This handles Admin, Doctor, Customer, etc.)
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            // 2. Map for the Main App (Default landing page)
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
