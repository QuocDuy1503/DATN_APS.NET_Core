using System.IO;
using System.Linq;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class DiemController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DiemController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var hocKyList = _context.HocKis
                .OrderByDescending(h => h.NamBatDau)
                .ThenByDescending(h => h.MaHocKi)
                .Select(h => new SelectListItem
                {
                    Value = h.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(h.MaHocKi)
                        ? $"HK {h.NamBatDau}-{h.NamKetThuc}"
                        : h.MaHocKi
                })
                .ToList();

            ViewBag.HocKyList = hocKyList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(IFormFile? file, int? hocKy)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng ch?n file ?i?m.";
                return RedirectToAction(nameof(Index));
            }

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var allowed = new[] { ".csv", ".xls", ".xlsx" };
            if (ext == null || !allowed.Contains(ext))
            {
                TempData["ErrorMessage"] = "??nh d?ng file không h?p l?. Ch? ch?p nh?n .csv, .xls, .xlsx.";
                return RedirectToAction(nameof(Index));
            }

            // TODO: X? lý import và l?u DB (s? b? sung controller logic sau)

            TempData["SuccessMessage"] = "File h?p l?, s?n sàng x? lý import.";
            return RedirectToAction(nameof(Index));
        }
    }
}
