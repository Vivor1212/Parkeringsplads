using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("Trip")]
public partial class Trip
{
    [Key]
    [Column("Trip_Id")]
    public int TripId { get; set; }

    [Required]
    [Column("From_Destination")]
    [StringLength(40)]
    [Unicode(false)]
    public string FromDestination { get; set; }

    [Required]
    [Column("To_Destination")]
    [StringLength(40)]
    [Unicode(false)]
    public string ToDestination { get; set; }

    [Column("Trip_Date")]
    public DateOnly TripDate { get; set; }

    [Column("Trip_Seats")]
    public int TripSeats { get; set; }

    [Column("Trip_Time")]
    [Precision(2)]
    public TimeOnly TripTime { get; set; }

    [Column("Car_Id")]
    public int CarId { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("Trips")]
    public virtual Car Car { get; set; }

    [InverseProperty("Trip")]
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

}
