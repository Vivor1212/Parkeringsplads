﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Parkeringsplads.Models;

[Table("Address")]
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
    [InverseProperty("Addresses")]
    public virtual City City { get; set; }

    [InverseProperty("Address")]
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();

    [InverseProperty("Address")]
    public virtual ICollection<School> Schools { get; set; } = new List<School>();

    public string FullAddress => $"{AddressRoad} {AddressNumber}, {City?.PostalCode} {City?.CityName}";

}
