﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkeringsplads.Models;

[Table("Request")]
public partial class Request
{
    [Key]
    [Column("Request_Id")]
    public int RequestId { get; set; }

    [Column("Request_Status")]
    public bool? RequestStatus { get; set; }

    [Column("Request_Time")]
    [Precision(2)]
    public TimeOnly RequestTime { get; set; }

    [Column("Request_Address")]
    [StringLength(100)]
    [Unicode(false)]
    public string? RequestAddress { get; set; }

    [Column("Request_Message")]
    [StringLength(255)]
    [Unicode(false)]
    public string? RequestMessage { get; set; }

    [Column("User_Id")]
    public int? UserId { get; set; }

    [Column("Trip_Id")]
    public int TripId { get; set; }

    [ForeignKey("TripId")]
    [InverseProperty("Requests")]
    public virtual Trip Trip { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Requests")]
    public virtual User Users { get; set; }
}
