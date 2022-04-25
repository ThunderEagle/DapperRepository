using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Models {
  [Table("Addresses")]
  public class Address {
    [Key, Column("Id")]
    public virtual int Id { get; set; }
    [Column("LastDate")]
    public virtual DateTime LastDate { get; set; }
    [Column("Line1")]
    public virtual string Line1 { get; set; }
    [Column("Line2")]
    public virtual string? Line2 { get; set; }
    [Column("Line3")]
    public virtual string? Line3 { get; set; }
    [Column("City")]
    public virtual string City { get; set; }
    [Column("State")]
    public virtual string State { get; set; }
    [Column("PostalCode")]
    public virtual string PostalCode { get; set; }
    [Column("Country")]
    public virtual string? Country { get; set; }
    [Column("County")]
    public virtual string? County { get; set; }
        
  }
}
