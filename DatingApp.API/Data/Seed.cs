using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        // {

        // }
        
        public static async Task SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // Create some roles
                var roles = new List<Role>
                {
                    new Role { Name = "Member" },
                    new Role { Name = "Admin" },
                    new Role { Name = "Moderator" },
                    new Role { Name = "VIP" }
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                    //roleManager.CreateAsync(role).Wait();
                }

                foreach (var user in users)
                {
                    user.Photos.SingleOrDefault().IsApproved = true;
                    
                    await userManager.CreateAsync(user, "password");
                    //userManager.CreateAsync(user, "password").Wait();
                    
                    await userManager.AddToRoleAsync(user, "Member");
                    //userManager.AddToRoleAsync(user, "Member");

                    // byte[] passwordHash, passwordSalt;
                    // CreatePasswordHash("password", out passwordHash, out passwordSalt);

                    // // user.PasswordHash = passwordHash;
                    // // user.PasswordSalt = passwordSalt;
                    // user.UserName = user.UserName.ToLower();
                    // context.Users.Add(user);
                }

                // Create admin user
                var adminUser = new User
                {
                    UserName = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, "password");
                //var result = userManager.CreateAsync(adminUser, "password").Result;

                if (result.Succeeded)
                {
                    var admin = await userManager.FindByNameAsync("Admin");
                    //var admin = userManager.FindByNameAsync("Admin").Result;
                    
                    await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
                    //userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });
                }

                // context.SaveChanges();
            }
        }

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }            
        }
    }
}