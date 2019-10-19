using System;
using System.ComponentModel.DataAnnotations.Schema;
using Promobile.Models.Utils;

namespace Promobile.Models
{
    public class PromotionalProduct
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string FeaturedOptionalText { get; set; }

        public string ImagePath { get; set; }

        public string QrCodeString { get; set; }

        public virtual ProductCategory ProductCategory { get; set; }

        public decimal InCashPrice { get; set; }

        public decimal ForwardedPrice { get; set; }

        public decimal ParcelValue { get; set; }

        public int InputParcelsQuantity { get; set; }

        public int ParcelsQuantity { get; set; }

        public decimal YearlyTax { get; set; }

        public decimal MonthlyTax { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiryDate { get; set; }

        public Status Status { get; set; }

        public RegionDivision RegionDivision { get; set; }

        public string ProductHash { get; set; }

    }
}