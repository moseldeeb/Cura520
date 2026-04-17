using Cura520.DataAccess;
using Cura520.Models;
using Cura520.Repos;
using Cura520.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Cura520
{
    public static class AppConfiguration
    {

        public static void Config(this IServiceCollection services , string connectionString )
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
               "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.SignIn.RequireConfirmedEmail = true;
            })
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
                options.LogoutPath = $"/Identity/Account/Logout";
            });

            // Email Service
            services.AddTransient<IEmailSender, EmailSender>();

            // Database Initializer
            services.AddScoped<IDBInitializr, DBInitializr>();

            // --- REPOSITORIES ---
            // User Profiles
            services.AddScoped<IRepository<Doctor>, Repository<Doctor>>();
            services.AddScoped<IRepository<Patient>, Repository<Patient>>();
            services.AddScoped<IRepository<Receptionist>, Repository<Receptionist>>();

            // Core Logic
            services.AddScoped<IRepository<Appointment>, Repository<Appointment>>();
            services.AddScoped<IRepository<DoctorSchedule>, Repository<DoctorSchedule>>();
            services.AddScoped<IRepository<MedicalHistory>, Repository<MedicalHistory>>();

            // Billing & Services
            services.AddScoped<IRepository<Invoice>, Repository<Invoice>>();
            services.AddScoped<IRepository<InvoiceItem>, Repository<InvoiceItem>>();
            services.AddScoped<IRepository<MedicalService>, Repository<MedicalService>>();

            // Pharmacy/Prescriptions
            services.AddScoped<IRepository<Prescription>, Repository<Prescription>>();
            services.AddScoped<IRepository<PrescriptionItem>, Repository<PrescriptionItem>>();

            // Security
            services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();



            

        }
    }
}
