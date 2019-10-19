using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Promobile.Data;
using Promobile.Models.Utils;
using Promobile.ViewModels;

namespace Promobile.Controllers
{
    public class HomeController : Controller
    {
        private readonly PromobileContext _db;

        public HomeController(PromobileContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Index(string errorMessage, string successMessage)
        {
            if (errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            if (successMessage != null)
            {
                ViewBag.SuccessMessage = successMessage;
            }

            var regionDivision = (RegionDivision)Enum.Parse(typeof(RegionDivision), $"{User.Claims.Where(c => c.Type == "RegionDivision").Select(c => c.Value).FirstOrDefault()}");

            var viewModel = new HomeViewModel
            {
                PromotionalProducts = await _db.PromotionalProducts.Where(p => p.RegionDivision == regionDivision).ToListAsync(),
                Banners = await _db.Banners.Where(b => b.RegionDivision == regionDivision).ToListAsync()
            };

            switch (regionDivision)
            {
                case RegionDivision.Midwest:
                    viewModel.RegionName = "Centro-Oeste";
                    break;
                case RegionDivision.North:
                    viewModel.RegionName = "Norte";
                    break;
                case RegionDivision.Northeast:
                    viewModel.RegionName = "Nordeste";
                    break;
                case RegionDivision.South:
                    viewModel.RegionName = "Sul";
                    break;
                case RegionDivision.Southeast:
                    viewModel.RegionName = "Sudeste";
                    break;
                default:
                    viewModel.RegionName = "Virtual";
                    break;
            }

            return View(viewModel);
        }
    }
}
