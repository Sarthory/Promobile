using System;
using System.ComponentModel.DataAnnotations.Schema;
using Promobile.Models.Utils;

namespace Promobile.Models
{
    public class Banner
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string ImagePath { get; set; }

        public string MobileImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiryDate { get; set; }

        public Status Status { get; set; }

        public RegionDivision RegionDivision { get; set; }

    }
}
