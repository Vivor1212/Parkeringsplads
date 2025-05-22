namespace Parkeringsplads.Models
{
    public class TripValidation
    {
        public bool IsValid { get; set; }
        public Trip? Trip { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RedirectPage { get; set; }
    }
}
