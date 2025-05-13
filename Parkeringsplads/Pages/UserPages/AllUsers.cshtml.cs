using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.UserPages
{
    public class AllUsersModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IUser _userService;

        public AllUsersModel(ParkeringspladsContext context, IUser userService)
        {
            _userService = userService;
            _context = context;
        }

        public List<User> Users { get; set; }
        public List<UserAddress> UserAddresses { get; set; }

        public List<Address> Addresses { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _userService.GetAllUsersAsync();

        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _userService.DeleteUserAsync(id);

            if (!success)
            {
                // Optional: handle failure (e.g., user not found)
            }

            return RedirectToPage();
        }


    }
}
