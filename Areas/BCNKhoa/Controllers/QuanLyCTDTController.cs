using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using ClosedXML.Excel;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyCTDTController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyCTDTController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? khoaId, string? searchString)
        {
            var query = _context.ChuongTrinhDaoTaos
                .Include(ct => ct.IdKhoaHocNavigation)
                .AsQueryable();

            if (khoaId.HasValue)
            {
                query = query.Where(ct => ct.IdKhoaHoc == khoaId);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(ct => ct.MaCtdt.Contains(searchString) || (ct.TenCtdt ?? "").Contains(searchString));
            }

            var items = await query
                .OrderBy(ct => ct.SttHienThi)
                .ThenBy(ct => ct.MaCtdt)
                .Select(ct => new CTDTItem
                {
                    Id = ct.Id,
                    MaCtdt = ct.MaCtdt,
                    TenCtdt = ct.TenCtdt ?? "",
                    Nganh = "",
                    Khoa = ct.IdKhoaHocNavigation != null && ct.IdKhoaHocNavigation.TenKhoa != null ? ct.IdKhoaHocNavigation.TenKhoa : "",
                    TongTinChi = ct.TongTinChi ?? 0,
                    TrangThai = ct.TrangThai ?? false
                })
                .ToListAsync();

            var vm = new CTDTViewModel
            {
                KhoaHocId = khoaId,
                SearchString = searchString,
                ChuongTrinhs = items,
                KhoaOptions = await _context.KhoaHocs
                    .OrderByDescending(k => k.Id)
                    .Select(k => new SelectListItem { Value = k.Id.ToString(), Text = k.TenKhoa ?? $"K{k.Id}" })
                    .ToListAsync()
            };

            ViewBag.CurrentKhoaId = khoaId;
            ViewBag.CurrentFilter = searchString;
            ViewBag.ListKhoaHoc = vm.KhoaOptions;
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ct = await _context.ChuongTrinhDaoTaos
                .Include(c => c.IdKhoaHocNavigation)
                .Include(c => c.ChiTietCtdts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (ct == null) return NotFound();

            var detail = new CTDTDetailViewModel
            {
                Id = ct.Id,
                MaCtdt = ct.MaCtdt,
                TenCtdt = ct.TenCtdt ?? "",
                Khoa = ct.IdKhoaHocNavigation?.TenKhoa ?? "",
                TongTinChi = ct.TongTinChi ?? 0,
                HocPhans = ct.ChiTietCtdts
                    .OrderBy(hp => hp.HocKiToChuc)
                    .ThenBy(hp => hp.Stt)
                    .Select(hp => new CTDTHocPhanViewModel
                    {
                        Stt = hp.Stt ?? 0,
                        MaHocPhan = hp.MaHocPhan ?? "",
                        TenHocPhan = hp.TenHocPhan ?? "",
                        SoTinChi = hp.SoTinChi ?? 0,
                        LoaiHocPhan = hp.LoaiHocPhan ?? "",
                        DieuKienTienQuyet = hp.DieuKienTienQuyet,
                        HocKiToChuc = hp.HocKiToChuc ?? 0
                    }).ToList()
            };

            return View(detail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? file, int? khoaHocId, string? maCtdt, string? tenCtdt)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file Excel." });

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
                return Json(new { success = false, message = "Định dạng file không hợp lệ. Vui lòng chọn file .xlsx hoặc .xls." });

            // Đọc và kiểm tra dữ liệu trước
            var parsedChiTiet = new List<ChiTietCtdt>();
            int tongTinChi = 0;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        return Json(new { success = false, message = "File không có sheet dữ liệu." });

                    var lastRow = worksheet.LastRowUsed();
                    if (lastRow == null)
                        return Json(new { success = false, message = "File Excel trống." });

                    int rowCount = lastRow.RowNumber();
                    int startRow = 6; // theo mẫu
                    int currentHocKy = 0;

                    for (int row = startRow; row <= rowCount; row++)
                    {
                        var maHp = worksheet.Cell(row, 4).GetValue<string>().Trim(); // Cột D
                        if (string.IsNullOrEmpty(maHp)) continue;

                        var valHocKy = worksheet.Cell(row, 1).GetValue<string>().Trim(); // Cột A
                        if (!string.IsNullOrEmpty(valHocKy) && int.TryParse(valHocKy, out var hk))
                        {
                            currentHocKy = hk;
                        }

                        int.TryParse(worksheet.Cell(row, 3).GetValue<string>(), out var stt); // Cột C
                        int.TryParse(worksheet.Cell(row, 6).GetValue<string>(), out var soTc); // Cột F

                        var chiTiet = new ChiTietCtdt
                        {
                            Stt = stt,
                            MaHocPhan = maHp,
                            TenHocPhan = worksheet.Cell(row, 5).GetValue<string>().Trim(), // Cột E
                            SoTinChi = soTc,
                            LoaiHocPhan = worksheet.Cell(row, 11).GetValue<string>().Trim(), // Cột K
                            DieuKienTienQuyet = worksheet.Cell(row, 12).GetValue<string>().Trim(), // Cột L
                            HocKiToChuc = currentHocKy == 0 ? null : currentHocKy
                        };

                        parsedChiTiet.Add(chiTiet);
                        tongTinChi += soTc;
                    }
                }
            }

            if (!parsedChiTiet.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu học phần trong file." });
            }

            // Xóa dữ liệu CTDT trùng (cùng khóa hoặc cùng mã) sau khi file hợp lệ
            var targetKhoaId = khoaHocId;
            var existingQuery = _context.ChuongTrinhDaoTaos.AsQueryable();
            if (targetKhoaId.HasValue)
                existingQuery = existingQuery.Where(ct => ct.IdKhoaHoc == targetKhoaId.Value);
            if (!string.IsNullOrWhiteSpace(maCtdt))
                existingQuery = existingQuery.Where(ct => ct.MaCtdt == maCtdt);

            var existingIds = await existingQuery.Select(ct => ct.Id).ToListAsync();
            if (existingIds.Any())
            {
                var details = _context.ChiTietCtdts.Where(d => d.IdCtdt != null && existingIds.Contains(d.IdCtdt.Value));
                _context.ChiTietCtdts.RemoveRange(details);
                var programs = _context.ChuongTrinhDaoTaos.Where(ct => existingIds.Contains(ct.Id));
                _context.ChuongTrinhDaoTaos.RemoveRange(programs);
                await _context.SaveChangesAsync();
            }

            // Thêm mới CTDT và chi tiết
            var newCtdt = new ChuongTrinhDaoTao
            {
                MaCtdt = maCtdt ?? Path.GetFileNameWithoutExtension(file.FileName),
                TenCtdt = tenCtdt ?? "Chương trình đào tạo mới",
                IdKhoaHoc = targetKhoaId,
                TongTinChi = 0,
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            _context.ChuongTrinhDaoTaos.Add(newCtdt);
            await _context.SaveChangesAsync();

            foreach (var ct in parsedChiTiet)
            {
                ct.IdCtdt = newCtdt.Id;
            }

            _context.ChiTietCtdts.AddRange(parsedChiTiet);
            newCtdt.TongTinChi = tongTinChi;
            _context.ChuongTrinhDaoTaos.Update(newCtdt);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã import và ghi đè dữ liệu CTĐT thành công." });
        }
    }
}
