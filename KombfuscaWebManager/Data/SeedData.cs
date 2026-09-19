using KombfuscaWebManager.Models;
using Microsoft.AspNetCore.Identity;

namespace KombfuscaWebManager.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider services, IConfiguration configuration)
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
            var email = configuration["SeedAdmin:Email"];
            var password = configuration["SeedAdmin:Password"];
            var fullName = configuration["SeedAdmin:FullName"];

            // Roles are always created. The initial administrator is optional and
            // must be supplied through secrets/environment variables.
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var admin =
            await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? "Administrador" : fullName
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
