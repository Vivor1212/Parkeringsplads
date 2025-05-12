using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.UserPages
{
    public class MyRequestsModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IRequestService _userService;

        public MyRequestsModel(ParkeringspladsContext context, IRequestService userService)
        {
            _userService = userService;
            _context = context;
        }

        public List<Request> Requests { get; set; }

        public List<User> Users { get; set; }
        public List<UserAddress> UserAddresses { get; set; }

        public List<Address> Addresses { get; set; }

        public List<Trip> Trips { get; set; }

        public string GetRequestStatusText(bool? status)
        {
            if (status == null) return "Pending";
            return status == true ? "Accepted" : "Rejected";
        }

        public async Task<IActionResult> OnGetAsync()
        {

            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                // If no user is logged in, redirect to login page
                return RedirectToPage("/Account/Login/Login");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);

            Requests = await _userService.GetAllRequestsForUser(user);

            return Page();
        }

        public async Task OnPost()
        {
      
          
        }


    }
}
