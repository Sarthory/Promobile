using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Promobile.Data;
using Promobile.Extensions;
using Promobile.Models;
using Promobile.Models.Utils;
using QRCoder;

namespace Promobile.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PromobileContext _db;
        private readonly IHostingEnvironment _environment;

        public ProductsController(PromobileContext db, IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _environment = hostingEnvironment;
        }

        [Authorize]
        public IActionResult Create(string errorMessage, string successMessage)
        {
            if (errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            if (successMessage != null)
            {
                ViewBag.SuccessMessage = successMessage;
            }

            ViewBag.Categories = _db.ProductCategories.ToList();

            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult CreateProduct(IFormCollection formData, IFormFile productImage)
        {
            var acceptedMimes = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

            using (var dbTransaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var productName = $"{formData["ProductName"]}";
                    var productHash = $"{Guid.NewGuid()}";
                    var featuredText = $"{formData["FeaturedText"]}";
                    var productCategory = Convert.ToInt32(formData["ProductCategory"]);
                    var inCashPrice = Convert.ToDecimal(Convert.ToString(formData["InCashPrice"]).Replace("R$ ", ""));
                    var parcelValue = Convert.ToDecimal(Convert.ToString(formData["ParcelValue"]).Replace("R$ ", ""));
                    var forwardedPrice = Convert.ToDecimal(Convert.ToString(formData["ForwardedPrice"]).Replace("R$ ", ""));
                    var yearlyTax = Convert.ToDecimal(Convert.ToString(formData["YearlyTax"]).Replace("%", ""));
                    var monthlyTax = Convert.ToDecimal(Convert.ToString(formData["MonthlyTax"]).Replace("%", ""));
                    var inputParcelsQuantity = Convert.ToInt32(formData["InputParcelsQuantity"]);
                    var parcelsQuantity = Convert.ToInt32(formData["ParcelsQuantity"]);
                    var expiryDate = DateTime.Parse($"{formData["ExpiryDate"]}");
                    var imagePath = string.Empty;

                    var qrGenerator = new QRCodeGenerator();
                    var qrCodeData = qrGenerator.CreateQrCode(productHash, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new Base64QRCode(qrCodeData);
                    var qrCodeString = qrCode.GetGraphic(10);

                    if (productImage != null)
                    {
                        var imageExtension = $".{productImage.FileName.Split('.')[1]}";

                        var bannerMimeMatch = acceptedMimes.FirstOrDefault(check => check.Contains(imageExtension));

                        if (bannerMimeMatch == null)
                        {
                            throw new Exception("Apenas imagens no formato *.jpg, *.jpeg, *.gif ou *.png podem ser cadastradas!");
                        }

                        var fileName = $"{Guid.NewGuid() + imageExtension}";

                        var newFileName = Path.Combine(_environment.WebRootPath, "img\\products") + $@"\{fileName}";

                        imagePath = $"http://www.pro-mobile.ml/img/products/{fileName}";

                        using (var fs = System.IO.File.Create(newFileName))
                        {
                            productImage.CopyTo(fs);
                            fs.Flush();
                        }
                    }

                    var newProduct = new PromotionalProduct
                    {
                        ProductCategory = _db.ProductCategories.FirstOrDefault(c => c.Id == productCategory),
                        ProductHash = productHash,
                        Name = productName,
                        FeaturedOptionalText = featuredText,
                        CreatedAt = DateTime.Now,
                        ExpiryDate = expiryDate,
                        ForwardedPrice = forwardedPrice,
                        InCashPrice = inCashPrice,
                        ParcelValue = parcelValue,
                        ImagePath = imagePath,
                        InputParcelsQuantity = inputParcelsQuantity,
                        ParcelsQuantity = parcelsQuantity,
                        MonthlyTax = monthlyTax,
                        YearlyTax = yearlyTax,
                        Status = Status.Active,
                        RegionDivision = (RegionDivision)Enum.Parse(typeof(RegionDivision), $"{User.Claims.Where(c => c.Type == "RegionDivision").Select(c => c.Value).FirstOrDefault()}"),
                        QrCodeString = qrCodeString
                    };

                    _db.PromotionalProducts.Add(newProduct);
                    _db.SaveChanges();
                    dbTransaction.Commit();
                }
                catch (Exception e)
                {
                    dbTransaction.Rollback();
                    return RedirectToAction("Create", "Products", new { errorMessage = $"Houve um erro ao cadastrar e salvar o novo produto! Erro: {e.Message}" });
                }
            }

            return RedirectToAction("Create", "Products", new { successMessage = $"Novo produto cadastrado com sucesso!" });
        }

        [Authorize]
        public IActionResult Edit(int? pId, string errorMessage, string successMessage)
        {
            if (errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            if (successMessage != null)
            {
                ViewBag.SuccessMessage = successMessage;
            }

            PromotionalProduct existentProduct;

            if (pId != null)
            {
                existentProduct = _db.PromotionalProducts
                    .Include(c => c.ProductCategory)
                    .FirstOrDefault(p => p.Id == pId);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errorMessage = $"Houve um problema ao encontrar o produto informado." });
            }

            if (existentProduct == null)
            {
                return RedirectToAction("Index", "Home", new { errorMessage = $"Houve um problema ao encontrar o produto informado." });
            }

            return View(existentProduct);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult EditProduct(IFormCollection formData)
        {
            try
            {
                var productId = Convert.ToInt32(formData["ProductId"]);
                var productName = $"{formData["ProductName"]}";
                var featuredText = $"{formData["FeaturedText"]}";
                var inCashPrice = Convert.ToDecimal(Convert.ToString(formData["InCashPrice"]).Replace("R$ ", ""));
                var parcelValue = Convert.ToDecimal(Convert.ToString(formData["ParcelValue"]).Replace("R$ ", ""));
                var forwardedPrice = Convert.ToDecimal(Convert.ToString(formData["ForwardedPrice"]).Replace("R$ ", ""));
                var yearlyTax = Convert.ToDecimal(Convert.ToString(formData["YearlyTax"]).Replace("%", ""));
                var monthlyTax = Convert.ToDecimal(Convert.ToString(formData["MonthlyTax"]).Replace("%", ""));
                var inputParcelsQuantity = Convert.ToInt32(formData["InputParcelsQuantity"]);
                var parcelsQuantity = Convert.ToInt32(formData["ParcelsQuantity"]);
                var expiryDate = DateTime.Parse($"{formData["ExpiryDate"]}");
                var productStatus = (Status)Convert.ToInt32(formData["ProductStatus"]);

                var existentProduct = _db.PromotionalProducts.FirstOrDefault(p => p.Id == productId);

                if (existentProduct == null)
                {
                    throw new Exception("Houve um problema ao encontrar o produto a ser editado!");
                }

                existentProduct.Name = productName;
                existentProduct.FeaturedOptionalText = featuredText;
                existentProduct.InCashPrice = inCashPrice;
                existentProduct.ParcelValue = parcelValue;
                existentProduct.ForwardedPrice = forwardedPrice;
                existentProduct.YearlyTax = yearlyTax;
                existentProduct.MonthlyTax = monthlyTax;
                existentProduct.InputParcelsQuantity = inputParcelsQuantity;
                existentProduct.ParcelsQuantity = parcelsQuantity;
                existentProduct.ExpiryDate = expiryDate;
                existentProduct.Status = productStatus;

                _db.Entry(existentProduct).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception e)
            {

                return RedirectToAction("Index", "Home", new { errorMessage = $"Houve um erro ao cadastrar e salvar o novo produto! Erro: {e.Message}" });
            }

            return RedirectToAction("Index", "Home", new { successMessage = $"Novo produto cadastrado com sucesso!" });
        }

        /*[HttpPost]
        //public JsonResult RegionProductsList(string regionKey)
        public JsonResult RegionProductsList(JObject data)
        {
            //var keySplit = regionKey.Split("|");
            //var clientLogin = $"{keySplit[0]}";
            //var clientPass = $"{keySplit[1]}";

            var clientLogin = $"{data["login"]}";
            var clientPass = $"{data["password"]}";

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
                        QrCodeString = $"<img width='200' src='data:image/png; base64, {p.QrCodeString}' alt='Qr-Code'/>",
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

                return Json(new
                {
                    status = "success",
                    productsList,
                    bannersList
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }*/
    }
}