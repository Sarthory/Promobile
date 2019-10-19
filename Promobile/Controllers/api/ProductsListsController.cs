using System;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Promobile.Data;
using Promobile.Extensions;
using Promobile.Models.Utils;

namespace Promobile.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ProductsListsController : ControllerBase
    {
        private readonly PromobileContext _db;

        public ProductsListsController(PromobileContext db)
        {
            _db = db;
        }

        [DisableCors]
        [HttpGet("RegionProductsList")]
        public IActionResult RegionProductsList(string regionKey)
        {
            var keySplit = regionKey.Split("|");
            var clientLogin = $"{keySplit[0]}";
            var clientPass = $"{keySplit[1]}";

            try
            {
                var clientUser = _db.Users.FirstOrDefault(u => u.Login == clientLogin && u.Password == PromobileExtensions.ToHash(clientPass));

                if (clientUser == null)
                {
                    throw new Exception("Nenhum item para ser listado.");
                }

                if (clientUser.UserType != UserType.ClientUser)
                {
                    throw new Exception("O usuário informado não é do tipo cliente.");
                }

                var productsList = _db.PromotionalProducts
                    .Where(p => p.Status == Status.Active)
                    .Where(p => p.RegionDivision == clientUser.RegionDivision)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.FeaturedOptionalText,
                        p.ImagePath,
                        p.QrCodeString,
                        p.ProductCategory,
                        p.InCashPrice,
                        p.ForwardedPrice,
                        p.ParcelValue,
                        p.InputParcelsQuantity,
                        p.ParcelsQuantity,
                        p.YearlyTax,
                        p.MonthlyTax,
                        p.ProductHash
                    })
                    .ToList();

                var bannersList = _db.Banners
                    .Where(b => b.Status == Status.Active)
                    .Where(b => b.RegionDivision == clientUser.RegionDivision)
                    .Select(b => new
                    {
                        b.Id,
                        b.Name,
                        b.ImagePath,
                        b.MobileImagePath
                    })
                    .ToList();

                return Ok(new
                {
                    status = "success",
                    productsList,
                    bannersList
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }
    }
}