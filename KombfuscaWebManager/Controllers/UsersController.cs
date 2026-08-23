using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.UsersModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace KombfuscaWebManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [Authorize(Roles = Roles.Admin)]
        public IActionResult Index()
        {
            return View(_userManager.Users.ToList());
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new EditRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName!,
                FullName = user.FullName,

                Admin = roles.Contains(Roles.Admin),
                ScoreCounter = roles.Contains(Roles.ScoreCounter),
                Player = roles.Contains(Roles.Player)
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EditRoles(EditRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (model.Admin) await _userManager.AddToRoleAsync(user, Roles.Admin);

            if (model.ScoreCounter) await _userManager.AddToRoleAsync(user, Roles.ScoreCounter);

            if (model.Player) await _userManager.AddToRoleAsync(user, Roles.Player);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var temporaryPassword = GenerateTemporaryPassword();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                temporaryPassword
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return BadRequest(ModelState);
            }

            user.MustChangePassword = true;

            await _userManager.UpdateAsync(user);

            return View("PasswordReset", temporaryPassword);
        }

        private static string GenerateTemporaryPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string numbers = "23456789";
            const string special = "!@#$%&*";

            var chars = upper + lower + numbers + special;

            var password = new char[12];

            password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            password[2] = numbers[RandomNumberGenerator.GetInt32(numbers.Length)];
            password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            for (int i = 4; i < password.Length; i++)
            {
                password[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
            }

            // Embaralha os caracteres
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);

                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }

    }
}
