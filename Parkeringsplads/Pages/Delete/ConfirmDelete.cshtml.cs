using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Delete
{
    public class ConfirmDeleteModel : PageModel
    {
        private readonly IUser _userService;
        private readonly ISchoolService _schoolService;
        private readonly IRequestService _requestService;

        public ConfirmDeleteModel(IUser userService, ISchoolService schoolService, IRequestService requestService)
        {
            _userService = userService;
            _schoolService = schoolService;
            _requestService = requestService;
        }

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            switch (Type.ToLower())
            {
                case "user":
                    var users = await _userService.GetAllUsersAsync();
                    var user = users.FirstOrDefault(u => u.UserId == Id);
                    DisplayName = user != null ? $"{user.FirstName} {user.LastName}" : null;
                    break;

                case "request":
                    var request = await _requestService.GetRequestByIdAsync(Id);
                    DisplayName = request != null
                        ? $"{request.Trip?.FromDestination} → {request.Trip?.ToDestination}"
                        : null;
                    break;

                /* 
                 
                    case "school":
                    var school = await _schoolService.GetByIdAsync(Id);
                    DisplayName = school?.Name;
                    break; 
                */

                default:
                    return BadRequest("Unsupported type.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            switch (Type.ToLower())
            {
                case "user":
                    await _userService.DeleteUserAsync(Id);
                    break;

                case "request":
                    await _requestService.DeleteRequestAsync(Id);
                    break;

                /* case "school":
                    await _schoolService.DeleteSchoolAsync(Id);
                    break; */

                default:
                    return BadRequest("Unsupported type.");
            }

            // Dynamically redirect based on deletion type
            return Type.ToLower() switch
            {
                "user" => RedirectToPage("/UserPages/AllUsers"),
                "request" => RedirectToPage("/UserPages/MyRequests"),
                _ => RedirectToPage("/Index")
            };
        }
    }
}
