namespace Parkeringsplads.Models
{
    public class UserValidation
    {
        public bool IsValid { get; set; }
        public User? User { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RedirectPage { get; set; }
    }
}
