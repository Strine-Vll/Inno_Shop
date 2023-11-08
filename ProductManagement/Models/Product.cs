using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Models
{
    [Table("product", Schema = "dbo")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("product_name")]
        [MinLength(2, ErrorMessage = "Product name must contain at least 2 characters")]
        public string ProductName { get; set; }

        [Column("description")]
        [MinLength(7, ErrorMessage = "Description must contain at least 7 characters")]
        public string Description { get; set; }

        [Column("price")]
        [Range(0, double.PositiveInfinity)]
        public decimal Price { get; set; }

        [Column("is_available")]
        public bool isAvailable { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("creation_date")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}
