using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Promobile.Data;
using Promobile.Models;
using Promobile.Models.Utils;

namespace Promobile.Controllers
{
    public class BannersController : Controller
    {
        private readonly PromobileContext _db;
        private readonly IHostingEnvironment _environment;

        public BannersController(PromobileContext db, IHostingEnvironment hostingEnvironment)
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

            return View();
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult CreateBanner(IFormCollection formData, IFormFile bannerImage, IFormFile mobileBannerImage)
        {
            var bannerName = $"{formData["BannerName"]}";
            var expiryDate = DateTime.Parse($"{formData["ExpiryDate"]}");
            var bannerImagePath = string.Empty;
            var mobileBannerImagePath = string.Empty;

            var acceptedMimes = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

            try
            {
                var mobileBannerImageExtension = $".{mobileBannerImage.FileName.Split('.')[1]}";
                var bannerImageExtension = $".{bannerImage.FileName.Split('.')[1]}";

                var mobileMimeMatch = acceptedMimes.FirstOrDefault(check => check.Contains(mobileBannerImageExtension));
                var bannerMimeMatch = acceptedMimes.FirstOrDefault(check => check.Contains(bannerImageExtension));

                if (mobileMimeMatch == null || bannerMimeMatch == null)
                {
                    throw new Exception("Apenas imagens no formato *.jpg, *.jpeg, *.gif ou *.png podem ser cadastradas!");
                }

                if (bannerImage != null)
                {
                    var fileName = $"{Guid.NewGuid() + bannerImageExtension}";

                    var newFileName = Path.Combine(_environment.WebRootPath, "img\\banners") + $@"\{fileName}";

                    bannerImagePath = $"http://www.pro-mobile.ml/img/banners/{fileName}";

                    using (FileStream fs = System.IO.File.Create(newFileName))
                    {
                        bannerImage.CopyTo(fs);
                        fs.Flush();
                    }
                }

                if (mobileBannerImage != null)
                {
                    var fileName = $"{Guid.NewGuid() + mobileBannerImageExtension}";

                    var newFileName = Path.Combine(_environment.WebRootPath, "img\\banners") + $@"\{fileName}";

                    mobileBannerImagePath = $"http://www.pro-mobile.ml/img/banners/{fileName}";

                    using (FileStream fs = System.IO.File.Create(newFileName))
                    {
                        mobileBannerImage.CopyTo(fs);
                        fs.Flush();
                    }
                }

                var newBanner = new Banner
                {
                    Status = Status.Active,
                    Name = bannerName,
                    RegionDivision = (RegionDivision)Enum.Parse(typeof(RegionDivision), $"{User.Claims.Where(c => c.Type == "RegionDivision").Select(c => c.Value).FirstOrDefault()}"),
                    CreatedAt = DateTime.Now,
                    ExpiryDate = expiryDate,
                    ImagePath = bannerImagePath,
                    MobileImagePath = mobileBannerImagePath
                };

                _db.Banners.Add(newBanner);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                return RedirectToAction("Create", "Banners", new { errorMessage = $"Houve um erro ao cadastrar e salvar o novo banner! Erro: {e.Message}" });
            }

            return RedirectToAction("Create", "Banners", new { successMessage = $"Novo banner cadastrado com sucesso!" });
        }

        [Authorize]
        public IActionResult Edit(int? bId, string errorMessage, string successMessage)
        {
            if (errorMessage != null)
            {
                ViewBag.ErrorMessage = errorMessage;
            }

            if (successMessage != null)
            {
                ViewBag.SuccessMessage = successMessage;
            }

            Banner existentBanner;

            if (bId != null)
            {
                existentBanner = _db.Banners.FirstOrDefault(b => b.Id == bId);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { errorMessage = $"Houve um problema ao encontrar o banner informado." });
            }

            if (existentBanner == null)
            {
                return RedirectToAction("Index", "Home", new { errorMessage = $"Houve um problema ao encontrar o banner informado." });
            }

            return View(existentBanner);
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult EditBanner(IFormCollection formData)
        {
            var bannerId = Convert.ToInt32(formData["BannerId"]);
            var bannerName = $"{formData["BannerName"]}";
            var expiryDate = DateTime.Parse($"{formData["ExpiryDate"]}");
            var bannerStatus = (Status)Convert.ToInt32(formData["BannerStatus"]);

            try
            {
                var existentBanner = _db.Banners.FirstOrDefault(b => b.Id == bannerId);

                if (existentBanner == null)
                {
                    throw new Exception("Houve um problema ao encontrar o banner a ser editado!");
                }

                existentBanner.Name = bannerName;
                existentBanner.ExpiryDate = expiryDate;
                existentBanner.Status = bannerStatus;

                _db.Entry(existentBanner).State = EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Home", new { errorMessage = $"Houve um erro ao editar e salvar o banner! Erro: {e.Message}" });
            }

            return RedirectToAction("Index", "Home", new { successMessage = $"O banner foi alterado com sucesso!" });
        }
    }
}