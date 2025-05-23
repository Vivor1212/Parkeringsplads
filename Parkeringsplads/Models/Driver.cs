using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("Driver")]
public partial class Driver
{
    [Key]
    [Column("Driver_Id")]
    public int DriverId { get; set; }

    [Required]
    [Column("Driver_License")]
    [StringLength(15)]
    [Unicode(false)]
    public string DriverLicense { get; set; }

    [Required]
    [Column("Driver_CPR")]
    [StringLength(10)]
    [Unicode(false)]
    public string DriverCpr { get; set; }

    [Column("User_Id")]
    public int UserId { get; set; }

    [Required]
    [Column("NumberOfPassengers")]
    public int NumberOfPassengers { get; set; } = 0;

    [ForeignKey("UserId")]
    [InverseProperty("Drivers")]
    public virtual User User { get; set; }

    [InverseProperty("Driver")]
    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();

    
}
