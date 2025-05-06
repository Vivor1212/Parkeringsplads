using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("School")]
public partial class School
{
    [Key]
    [Column("School_Id")]
    public int SchoolId { get; set; }

    [Required]
    [Column("School_Name")]
    [StringLength(30)]
    [Unicode(false)]
    public string SchoolName { get; set; }

    [Column("Address_Id")]
    public int AddressId { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("Schools")]
    public virtual Address Address { get; set; }

    [InverseProperty("School")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
