namespace Parkeringsplads.Models
{
    public class DriverValidation
    {
        public bool IsValid { get; set; }
        public Driver? Driver { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
