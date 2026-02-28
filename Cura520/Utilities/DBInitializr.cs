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
            // 1. Push pending migrations if any
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { /* Log error */ }

            // 2. Create roles if they don't exist
            if (!_roleManager.RoleExistsAsync(SD.Role_SuperAdmin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_SuperAdmin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Doctor)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Patient)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Staff)).GetAwaiter().GetResult();

                // 3. Create the "God Account" (Super Admin)
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@cura.com",
                    Email = "admin@cura.com",
                    FirstName = "Cura",
                    LastName = "Admin",
                    PhoneNumber = "0123456789",
                    Address = "Egypt, Cairo",
                    Type = UserType.Admin // Your enum
                }, "Admin123*").GetAwaiter().GetResult();

                ApplicationUser user = _db.Users.FirstOrDefault(u => u.Email == "admin@cura.com");
                _userManager.AddToRoleAsync(user, SD.Role_SuperAdmin).GetAwaiter().GetResult();
            }

            return;
        }
    }
}