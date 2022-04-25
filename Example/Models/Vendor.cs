using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Models {
  [Table("Vendors")]
  public class Vendor
  {
    [Key, Column("Id")]
    public virtual int Id { get; set; }
    [Column("LastDate")]
    public virtual DateTime LastDate { get; set; }
    [Column("Name")]
    public virtual string Name { get; set; }
    [Column("AddressId")]
    public virtual int? AddressID { get; set; }
    [Column("Status")]
    public virtual string Status { get; set; }
    [Column("PaymentType")]
    public virtual string PaymentType { get; set; }
    [Column("FederalId")]
    public virtual  string FederalId { get; set; }

    public virtual Address? Address { get; set; }

  }
}
