using AngularAuthAPI.Enums;
using AngularAuthAPI.Models;
using AngularAuthAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AngularAuthAPI.Controllers
{
    //[Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AdminController(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> RolesAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }


        public async Task<IActionResult> UsersAsync()
        {
            var applicationUsers = _userManager.Users.ToList();
            List<UserRolesViewModel> userRolesVm= new List<UserRolesViewModel>();
            foreach(ApplicationUser user in applicationUsers)
            {
                UserRolesViewModel urvm = new UserRolesViewModel
                {
                    UserId = user.Id,
                    FirstName= user.FirstName,
                    LastName= user.LastName,
                    Email= user.Email,
                    UserName= user.UserName,
                    Roles = (await _userManager.GetRolesAsync(user)).ToArray()
                };
                userRolesVm.Add(urvm);
            }

            return View(userRolesVm);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserAsync(string userId)
        {
            if (String.IsNullOrEmpty(userId)) return BadRequest("empty user id not allowed");
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(new
                {
                    Message = "User not found"
                });
            }
            if (await _userManager.IsInRoleAsync(user, Roles.SuperAdmin.ToString()))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new
                {
                    Message = "Cannot delete suepr admin"
                }); ;

            }

            await _userManager.DeleteAsync(user);

            return Json(new
            {
                Message = "SUCCESS",
                UserId = userId
            });
        }

        //[Route("Admin/Users/{id}")]
        [Route("Admin/Users/{id}")]
        public async Task<IActionResult> ManageUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["status"] = "Error: User not found";
                return RedirectToAction("Users");
            }
            var model = new List<ManageRoleViewModel>();
            var roles = await _roleManager.Roles.ToListAsync();
            foreach(var role in roles)
            {
                ManageRoleViewModel entry = new ManageRoleViewModel { RoleName = role.Name, IsSelected = false };
                if(await _userManager.IsInRoleAsync(user,role.Name)) entry.IsSelected= true;
                model.Add(entry);
            }
            ViewBag.UserId = user.Id;
            ViewBag.UserName = user.UserName;

            return View(model);
        }

        [HttpPost]
        [Route("Admin/Users/{id}")]
        public async Task<IActionResult> ManageUserAsync(IList<ManageRoleViewModel> model, string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return View();
            }
            var existingRoles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, existingRoles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }

            //adding new roles
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add new user roles");
                return View(model);
            }
            TempData["status"] = "Roles Updated";
            return View(model);

        }
    }
}
