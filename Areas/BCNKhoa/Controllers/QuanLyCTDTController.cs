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

        public async Task<IActionResult> Index(int? khoaId, string? searchString, int page = 1)
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

            const int pageSize = 12;
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var items = await query
                .OrderBy(ct => ct.SttHienThi)
                .ThenBy(ct => ct.MaCtdt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                Page = page,
                TotalPages = totalPages,
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

            var targetMaCtdt = string.IsNullOrWhiteSpace(maCtdt)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : maCtdt.Trim();

            if (string.IsNullOrWhiteSpace(targetMaCtdt))
                return Json(new { success = false, message = "Không xác định được mã CTĐT. Vui lòng nhập mã hoặc đặt lại tên file." });

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
                    int startRow = 3; // dòng dữ liệu đầu tiên sau tiêu đề (điều chỉnh nếu template thay đổi)
                    int currentHocKy = 0;

                    for (int row = startRow; row <= rowCount; row++)
                    {
                        var maHp = worksheet.Cell(row, 2).GetValue<string>().Trim(); // Cột B
                        if (string.IsNullOrEmpty(maHp)) continue;

                        var tenHp = worksheet.Cell(row, 3).GetValue<string>().Trim(); // Cột C
                        var loaiHp = worksheet.Cell(row, 9).GetValue<string>().Trim(); // Cột I
                        var dkTienQuyet = worksheet.Cell(row, 10).GetValue<string>().Trim(); // Cột J

                        var valHocKy = worksheet.Cell(row, 1).GetValue<string>().Trim(); // Cột A
                        if (!string.IsNullOrEmpty(valHocKy) && int.TryParse(valHocKy, out var hk))
                        {
                            currentHocKy = hk;
                        }

                        int.TryParse(worksheet.Cell(row, 1).GetValue<string>(), out var stt); // Cột A
                        int.TryParse(worksheet.Cell(row, 5).GetValue<string>(), out var soTc); // Cột E

                        int? hocKiToChuc = null;
                        var hocKiCell = worksheet.Cell(row, 12).GetValue<string>().Trim(); // Cột L
                        if (int.TryParse(hocKiCell, out var hkToChuc))
                        {
                            hocKiToChuc = hkToChuc;
                        }
                        else if (currentHocKy != 0)
                        {
                            hocKiToChuc = currentHocKy;
                        }

                        // Bỏ qua dòng nếu thiếu bất kỳ cột bắt buộc
                        if (string.IsNullOrWhiteSpace(tenHp) || string.IsNullOrWhiteSpace(loaiHp) || string.IsNullOrWhiteSpace(dkTienQuyet) || soTc <= 0 || hocKiToChuc == null)
                        {
                            continue;
                        }

                        var chiTiet = new ChiTietCtdt
                        {
                            Stt = stt,
                            MaHocPhan = maHp,
                            TenHocPhan = tenHp,
                            SoTinChi = soTc,
                            LoaiHocPhan = loaiHp,
                            DieuKienTienQuyet = dkTienQuyet,
                            HocKiToChuc = hocKiToChuc
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
            var existingIds = await _context.ChuongTrinhDaoTaos
                .Where(ct => ct.MaCtdt == targetMaCtdt || (targetKhoaId.HasValue && ct.IdKhoaHoc == targetKhoaId.Value))
                .Select(ct => ct.Id)
                .ToListAsync();
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
                MaCtdt = targetMaCtdt,
                TenCtdt = string.IsNullOrWhiteSpace(tenCtdt) ? "Chương trình đào tạo mới" : tenCtdt.Trim(),
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
