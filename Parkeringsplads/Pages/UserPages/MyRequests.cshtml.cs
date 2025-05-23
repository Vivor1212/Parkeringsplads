using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.UserPages
{
    public class MyRequestsModel : PageModel
    {
        private readonly IUser _userService;
        private readonly IRequestService _requestService;

        public MyRequestsModel(IUser userService, IRequestService requestService)
        {
            _userService = userService;
            _requestService = requestService;
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

            var user = await _userService.GetUserWithDetailsByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToPage("/Account/Login/Login");
            }

            Requests = await _requestService.GetAllRequestsForUser(user);
            return Page();
        }

        public async Task OnPost()
        {
      
          
        }


    }
}
