using System.Collections.Generic;
using Promobile.Models;

namespace Promobile.ViewModels
{
    public class HomeViewModel
    {
        public string RegionName { get; set; }

        public List<Banner> Banners { get; set; }

        public List<PromotionalProduct> PromotionalProducts { get; set; }

        public HomeViewModel()
        {
            Banners = new List<Banner>();
            PromotionalProducts = new List<PromotionalProduct>();
        }

    }
}
