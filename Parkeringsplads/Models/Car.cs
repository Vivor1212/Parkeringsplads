using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("Car")]
public partial class Car
{
    [Key]
    [Column("Car_Id")]
    public int CarId { get; set; }

    [Required]
    [Column("Car_Model")]
    [StringLength(30)]
    [Unicode(false)]
    public string CarModel { get; set; }

    [Required]
    [Column("Car_Plate")]
    [StringLength(15)]
    [Unicode(false)]
    public string CarPlate { get; set; }

    [Column("Car_Capacity")]
    public int CarCapacity { get; set; }

    [Column("Driver_Id")]
    public int DriverId { get; set; }

    [ForeignKey("DriverId")]
    [InverseProperty("Cars")]
    public virtual Driver Driver { get; set; }
}
