using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly IUser _userService;

        protected BasePageModel(IUser userService)
        {
            _userService = userService;
        }

        protected async Task<IActionResult> HandleValidationAndRedirect(UserValidation userResult)
        {
            if (!userResult.IsValid)
            {
                TempData["ErrorMessage"] = userResult.ErrorMessage;
                return RedirectToPage(userResult.RedirectPage);
            }
            return null;
        }
    }
}
