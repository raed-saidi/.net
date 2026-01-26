using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.BLL.DTOs;
using RealEstateMarketplace.DAL.Entities;

namespace RealEstateMarketplace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            });
        }
        
        return View(userDtos);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Json(new { success = false });
        }
        
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        
        return Json(new { success = true, isActive = user.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Json(new { success = false });
        }
        
        await _userManager.DeleteAsync(user);
        TempData["Success"] = "User deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
