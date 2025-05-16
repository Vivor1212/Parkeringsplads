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

        public string TitleText
        {
            get
            {
                return Title switch
                {
                    "P" => "Personale",
                    "S" => "Studerende",
                    _ => "Ukendt"
                };
            }
        }
        public bool IsDriver { get; set; }

        public School School { get; set; }
        
        public User User { get; set; }

        public List<Request> Requests { get; set; }

        public List<Address> AddressList { get; set; }


        public UserAddress UserAddress { get; set; }

        public string SchoolName { get; set; }

        public string GetRequestStatusText(bool? status)
        {
            if (status == null) return "Pending";
            return status == true ? "Accepted" : "Rejected";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if the user is an admin
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (!string.IsNullOrEmpty(isAdmin) && isAdmin == "true")
            {
                return RedirectToPage("/Admin/AdminDashboard"); 
            }

            // Retrieve user email from session
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

            AddressList = await _context.UserAddress
           .Where(ua => ua.User_Id == user.UserId)
            .Include(ua => ua.Address)
            .ThenInclude(ua => ua.City)
             .Select(ua => ua.Address)
             .ToListAsync();

            Requests = await _requestService.GetAllRequestsForUser(user); // ? Await the async method

            User = user;
            UserEmail = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Phone = user.Phone;
            Title= user.Title;
            School = user.School;
            SchoolName = user.School?.SchoolName;

            var driver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == user.UserId); // Async version
            IsDriver = driver != null;
            Driver = driver;

            return Page();
        }

    }
}
