using KombfuscaWebManager.Models;
using Microsoft.AspNetCore.Identity;

namespace KombfuscaWebManager.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles =
            {
            Roles.Admin,
            Roles.ScoreCounter,
            Roles.Player
        };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole(role));
                }
            }


            //Add Admin to Database
            const string email = "admin@kombfusca.com";
            const string password = "Admin@123456";

            var admin =
            await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result =
                    await userManager.CreateAsync(
                        admin,
                        password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(
                        admin,
                        Roles.Admin);
                }
            }
        }
    }
}
