using System.ComponentModel.DataAnnotations.Schema;

namespace Promobile.Models
{
    public class ProductCategory
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Description { get; set; }

        public Utils.ProductCategory Category { get; set; }

    }
}
