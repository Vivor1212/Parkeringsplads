using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("City")]
public partial class City
{
    [Key]
    [Column("City_Id")]
    public int CityId { get; set; }

    [Required]
    [Column("City_Name")]
    [StringLength(50)]
    [Unicode(false)]
    public string CityName { get; set; }

    [Required]
    [Column("Postal_Code")]
    [StringLength(4)]
    [Unicode(false)]
    public string PostalCode { get; set; }

    [InverseProperty("City")]
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
