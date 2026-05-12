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
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            string[] roles = {
                              SD.Role_SuperAdmin,
                              SD.Role_Admin,
                              SD.Role_Doctor,
                              SD.Role_Patient,
                              SD.Role_Receptionist
                            };

            foreach (var role in roles)
            {
                if (!_roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
                }
            }

            // Create SuperAdmin user if it doesn't already exist
            if (!_db.Users.Any(u => u.UserName == "CuraAdmin"))
            {
                try
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = "CuraAdmin",
                        Email = "admin@cura.com",
                        FirstName = "Cura",
                        LastName = "Admin",
                        PhoneNumber = "01090670584",
                        Address = "Egypt, Cairo",
                        EmailConfirmed = true,
                        Type = UserType.SuperAdmin
                    };
                    var result = _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();

                    if (result.Succeeded)
                    {
                        var roleResult = _userManager.AddToRoleAsync(adminUser, SD.Role_SuperAdmin).GetAwaiter().GetResult();
                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine("Error assigning SuperAdmin role:");
                            foreach (var error in roleResult.Errors)
                            {
                                Console.WriteLine($"- {error.Code}: {error.Description}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error creating SuperAdmin user:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Code}: {error.Description}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception creating SuperAdmin: {ex.Message}");
                }
            }

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

            // Create test Receptionist user if doesn't already exist
            if (!_db.Users.Any(u => u.UserName == "CuraStaff"))
            {
                try
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
                        var roleResult = _userManager.AddToRoleAsync(receptionistUser, SD.Role_Receptionist).GetAwaiter().GetResult();
                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine("Error assigning Receptionist role:");
                            foreach (var error in roleResult.Errors)
                            {
                                Console.WriteLine($"- {error.Code}: {error.Description}");
                            }
                        }

                        // Create receptionist profile
                        _db.Receptionists.Add(new Receptionist
                        {
                            FirstName = "Main",
                            LastName = "Reception",
                            ApplicationUserId = receptionistUser.Id
                        });

                        _db.SaveChanges();
                    }
                    else
                    {
                        Console.WriteLine("Error creating Receptionist user:");
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"- {error.Code}: {error.Description}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception creating Receptionist: {ex.Message}");
                }
            }

            return;
        }
    }
}