using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("UserAddress")]
public partial class UserAddress
{
    [Column("User_Id")]
    public int User_Id { get; set; }

    [Column("Address_Id")]
    public int Address_Id { get; set; }

    [ForeignKey("User_Id")]
    [InverseProperty("UserAddresses")]
    public virtual User User { get; set; }

    [ForeignKey("Address_Id")]
    [InverseProperty("UserAddresses")]
    public virtual Address Address { get; set; }
}
