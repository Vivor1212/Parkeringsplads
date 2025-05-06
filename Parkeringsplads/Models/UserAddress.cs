using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Parkeringsplads.Models
{
    [Table("UserAddress")]
    public partial class UserAddress
    {
        public int User_Id { get; set; }
        public int Address_Id { get; set; }

        public virtual User User { get; set; }
        public virtual Address Address { get; set; }
    }



}