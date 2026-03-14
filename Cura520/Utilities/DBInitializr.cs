using Cura520.DataAccess;
using Cura520.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cura520.Utilities
{
    public class DBInitializr : IDBInitializr
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DBInitializr(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }


        public void Initialize()
        {
            // 1. Push pending migrations
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Log error if needed: Console.WriteLine(ex.Message);
            }

            // 2. Create roles if they don't exist
            if (!_roleManager.RoleExistsAsync(SD.Role_SuperAdmin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_SuperAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Doctor)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Patient)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Receptionist)).GetAwaiter().GetResult();

                // 3. Create the "God Account" (Super Admin)
                var adminUser = new ApplicationUser
                {
                    UserName = "CuraAdmin",
                    Email = "admin@cura.com",
                    FirstName = "Cura",
                    LastName = "Admin",
                    PhoneNumber = "01090670584",
                    Address = "Egypt, Cairo",
                    EmailConfirmed = true,
                    Type = UserType.Admin
                };



                var result = _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();

                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(adminUser, SD.Role_SuperAdmin).GetAwaiter().GetResult();
                }
            }

            // 4. Seed basic medical services (ERP Price List)
            // This ensures your billing system has items to select from immediately
            if (!_db.MedicalServices.Any())
            {
                _db.MedicalServices.AddRange(new List<MedicalService>
        {
            new MedicalService { Name = "General Consultation", DefaultPrice = 200 },
            new MedicalService { Name = "Specialist Consultation", DefaultPrice = 400 },
            new MedicalService { Name = "Follow-up Visit", DefaultPrice = 100 },
            new MedicalService { Name = "Emergency Checkup", DefaultPrice = 600 },
            new MedicalService { Name = "Lab Test - Basic", DefaultPrice = 150 }
        });
                _db.SaveChanges();
            }

            // 5. Create a default Receptionist Profile (Optional but helpful for testing)
            // Link a user to the Receptionist table so the Area works right away
            if (!_db.Receptionists.Any())
            {
                var receptionistUser = new ApplicationUser
                {
                    UserName = "CuraStaff",
                    Email = "reception@cura.com",
                    FirstName = "Main",
                    LastName = "Reception",
                    EmailConfirmed = true,
                    Type = UserType.Receptionist
                };

                var result = _userManager.CreateAsync(receptionistUser, "Staff123*").GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(receptionistUser, SD.Role_Receptionist).GetAwaiter().GetResult();

                    _db.Receptionists.Add(new Receptionist
                    {
                        FirstName = "Main",
                        LastName = "Reception",
                        ApplicationUserId = receptionistUser.Id
                    });
                    _db.SaveChanges();
                }
            }

            return;
        }
    }
}