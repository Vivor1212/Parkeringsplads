using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Linq;

namespace Parkeringsplads.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IRequestService _requestService;
        public ProfileModel(ParkeringspladsContext context, IRequestService requestService)
        {
            _context = context;
            _requestService = requestService;
        }

        public Driver? Driver { get; set; }
        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }

        public string TitleText { get; set; }
        public bool IsDriver { get; set; }

        public School School { get; set; }
        
        public User User { get; set; }

        public List<Request> Requests { get; set; }

        public string SchoolName { get; set; }

        public string GetRequestStatusText(bool? status)
        {
            if (status == null) return "Pending";
            return status == true ? "Accepted" : "Rejected";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (!string.IsNullOrEmpty(isAdmin) && isAdmin == "true")
            {
                return RedirectToPage("/Admin/AdminDashboard"); 
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

            var user = await _context.User
                                     .Include(u => u.School)
                                     .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }

            Requests = await _requestService.GetAllRequestsForUser(user); 

            User = user;
            UserEmail = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Phone = user.Phone;
            TitleText = user.Title;
            School = user.School;
            SchoolName = user.School?.SchoolName;

            var driver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == user.UserId); 
            IsDriver = driver != null;
            Driver = driver;

            return Page();
        }

        public async Task<IActionResult> OnPostStopBeingDriverAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }

            var driver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == user.UserId);
            if (driver != null)
            {
                _context.Driver.Remove(driver);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("IsDriver");

                TempData["SuccessMessage"] = "You are no longer a driver.";
            }
            else
            {
                TempData["ErrorMessage"] = "You are not currently registered as a driver.";
            }

            return RedirectToPage("/Account/Profile");
        }


    }
}
