using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public partial class Address
{
    [Key]
    [Column("Address_Id")]
    public int AddressId { get; set; }

    [Required]
    [Column("Address_Road")]
    [StringLength(100)]
    [Unicode(false)]
    public string AddressRoad { get; set; }

    [Required]
    [Column("Address_Number")]
    [StringLength(10)]
    [Unicode(false)]
    public string AddressNumber { get; set; }

    [Column("City_Id")]
    public int CityId { get; set; }

    [ForeignKey("CityId")]
    [InverseProperty("Address")]
    public virtual City City { get; set; }

    // Change this to reference UserAddress instead of User directly
    [InverseProperty("Address")]
    public virtual ICollection<UserAddress> UserAddress { get; set; } = new List<UserAddress>();


    // Navigation property for Schools related to this address
    [InverseProperty("Address")]
    public virtual ICollection<School> Schools { get; set; } = new List<School>();

}
