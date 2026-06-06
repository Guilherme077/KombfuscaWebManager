using KombfuscaWebManager.Models;
using KombfuscaWebManager.Models.UsersModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

                Admin = roles.Contains(Roles.Admin),
                ScoreCounter = roles.Contains(Roles.ScoreCounter),
                Player = roles.Contains(Roles.Player)
            };

            return View(model);
        }

        [HttpPost]
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

    }
}
