using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Pages.TripPages;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;
using System.Linq;

namespace Parkeringsplads.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly IUser _userService;
        private readonly IDriverService _driverService;
        private readonly IRequestService _requestService;
        private readonly IAddressService _addressService;
        private readonly ITripService _tripService;

        public ProfileModel(IUser userService, IDriverService driverService, IRequestService requestService, IAddressService addressService, ITripService tripService)
        {
            _userService = userService;
            _driverService = driverService;
            _requestService = requestService;
            _addressService = addressService;
            _tripService = tripService;
        }

        public int NumberOfTrips { get; set; }
        public int NumberOfPassengers { get; set; }
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
                    "p" => "Personale",
                    "S" => "Studerende",
                    "s" => "Studerende",
                    _ => "Ukendt"
                };
            }
        }
        public bool IsDriver { get; set; }

        public School School { get; set; }
        
        public User User { get; set; }

        public List<Request> Requests { get; set; }

        public List<Trip> AllTripsOnUser { get; set; }

        public List<Trip> TodayTrips { get; set; }

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

            var user = await _userService.GetUserWithDetailsByEmailAsync(userEmail);


            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }
            
            AddressList = await _addressService.GetUserAddressesAsync(user.UserId);
            Requests = await _requestService.GetAllRequestsForUser(user);
            AllTripsOnUser = await _tripService.GetAllTripsOnUserAsync(user);

            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            TodayTrips = AllTripsOnUser?
                .Where(t => t.TripDate == today)
                .ToList() ?? new List<Trip>();

            User = user;
            UserEmail = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Phone = user.Phone;
            Title = user.Title;
            School = user.School;
            SchoolName = user.School?.SchoolName;


            var driver = await _driverService.GetDriverByUserIdAsync(user.UserId);
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

            var user = await _userService.GetUserWithDetailsByEmailAsync(userEmail);
            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }

            var success = await _driverService.UnbecomeDriver(user.UserId);
            if (success)
            {
                HttpContext.Session.Remove("IsDriver");
                TempData["SuccessMessage"] = "Du er ikke længere chauffør.";
            }
            else
            {
                TempData["ErrorMessage"] = "Du er ikke registreret som chauffør.";
            }

            return RedirectToPage("/Account/Profile");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, string type)
        {
            switch (type)
            {
                case "Address":
                    await _addressService.DeleteAddressAsync(id);
                    TempData["SuccessMessage"] = "Adressen blev slettet.";
                    break;

                case "Request":
                    await _requestService.DeleteRequestAsync(id);
                    TempData["SuccessMessage"] = "Anmodningen blev slettet.";
                    break;

                default:
                    TempData["ErrorMessage"] = "Ugyldig type til sletning.";
                    break;
            }
            return RedirectToPage();
        }
    }
}
