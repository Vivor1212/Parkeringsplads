using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public partial class User
{
    [Key]
    [Column("User_Id")]
    public int UserId { get; set; }

    [Required]
    [Column("First_Name")]
    [StringLength(30)]
    [Unicode(false)]
    public string FirstName { get; set; }

    [Required]
    [Column("Last_Name")]
    [StringLength(40)]
    [Unicode(false)]
    public string LastName { get; set; }

    [Required]
    [StringLength(255)]
    [Unicode(false)]
    public string Email { get; set; }

    [Required]
    [StringLength(15)]
    [Unicode(false)]
    public string Phone { get; set; }

    [Required]
    [StringLength(30)]
    [Unicode(false)]
    public string Password { get; set; }

    [Required]
    [StringLength(1)]
    [Unicode(false)]
    public string Title { get; set; }

    [Column("School_Id")]
    public int? SchoolId { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    [ForeignKey("SchoolId")]
    [InverseProperty("Users")]
    public virtual School? School { get; set; }

    // Change this to reference UserAddress instead of Address directly
    [InverseProperty("User")]
    public virtual ICollection<UserAddress> UserAddress { get; set; } = new List<UserAddress>();
}
