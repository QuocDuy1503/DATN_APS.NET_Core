using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using ClosedXML.Excel;
using System.Text.RegularExpressions;

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

            const int pageSize = 10;
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

            int tongTinChi = 0;
            var parsedChiTiet = new List<ChiTietCtdt>();
            var khoiImports = new List<(KhoiKienThuc Block, bool SumFromB, List<MonHoc> Subjects)>();
            (KhoiKienThuc Block, bool SumFromB, List<MonHoc> Subjects)? currentBlock = null;
            int sttCounter = 1;

            int? SafeParseInt(string? input)
            {
                if (string.IsNullOrWhiteSpace(input)) return null;
                input = input.Trim();
                if (int.TryParse(input, out var val)) return val;
                var match = Regex.Match(input, "-?\\d+");
                if (match.Success && int.TryParse(match.Value, out var inner)) return inner;
                return null;
            }

            string RemoveParentheses(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                return Regex.Replace(value, "\\s*\\(.*?\\)\\s*", string.Empty).Trim();
            }

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
                    int startRow = 1; // duyệt toàn bộ, tự nhận diện khối/môn theo logic yêu cầu

                    for (int row = startRow; row <= rowCount; row++)
                    {
                        var colA = worksheet.Cell(row, 1).GetValue<string>().Trim();
                        var colB = worksheet.Cell(row, 2).GetValue<string>().Trim();
                        var colC = worksheet.Cell(row, 3).GetValue<string>().Trim();
                        var colD = worksheet.Cell(row, 4).GetValue<string>().Trim();
                        var colE = worksheet.Cell(row, 5).GetValue<string>().Trim();
                        var colF = worksheet.Cell(row, 6).GetValue<string>().Trim();

                        bool rowEmpty = string.IsNullOrWhiteSpace(colA) && string.IsNullOrWhiteSpace(colB) && string.IsNullOrWhiteSpace(colC) && string.IsNullOrWhiteSpace(colD) && string.IsNullOrWhiteSpace(colE);
                        if (rowEmpty && currentBlock == null)
                        {
                            break; // kết thúc import khi không còn dòng hợp lệ
                        }

                        bool isBlockHeaderCaseA = !string.IsNullOrWhiteSpace(colB) && !string.IsNullOrWhiteSpace(colE) && string.IsNullOrWhiteSpace(colC);
                        bool isBlockHeaderCaseC = string.IsNullOrWhiteSpace(colB) && string.IsNullOrWhiteSpace(colE) && !string.IsNullOrWhiteSpace(colA) && string.IsNullOrWhiteSpace(colC);

                        if (isBlockHeaderCaseA || isBlockHeaderCaseC)
                        {
                            var khoi = new KhoiKienThuc
                            {
                                TenKhoi = isBlockHeaderCaseA ? RemoveParentheses(colB) : colA.Trim(),
                                TongTinChi = isBlockHeaderCaseA ? SafeParseInt(colE) ?? 0 : 0
                            };
                            currentBlock = (khoi, isBlockHeaderCaseC, new List<MonHoc>());
                            khoiImports.Add(currentBlock.Value);
                            continue;
                        }

                        if (rowEmpty)
                        {
                            currentBlock = null;
                            continue;
                        }

                        if (currentBlock == null && (!string.IsNullOrWhiteSpace(colB) || !string.IsNullOrWhiteSpace(colC) || !string.IsNullOrWhiteSpace(colD) || !string.IsNullOrWhiteSpace(colE)))
                        {
                            var khoi = new KhoiKienThuc { TenKhoi = "Khối kiến thức", TongTinChi = 0 };
                            currentBlock = (khoi, false, new List<MonHoc>());
                            khoiImports.Add(currentBlock.Value);
                        }

                        if (currentBlock == null)
                        {
                            continue; // bỏ qua các dòng ngoài khối
                        }

                        bool isSubjectRow = !string.IsNullOrWhiteSpace(colC) || !string.IsNullOrWhiteSpace(colB) || !string.IsNullOrWhiteSpace(colD) || !string.IsNullOrWhiteSpace(colE);
                        if (!isSubjectRow)
                        {
                            continue;
                        }

                        var soTinChi = SafeParseInt(colE) ?? SafeParseInt(colB);
                        var hocKi = SafeParseInt(colA);
                        var mon = new MonHoc
                        {
                            MaMon = string.IsNullOrWhiteSpace(colB) ? null : colB,
                            TenMon = string.IsNullOrWhiteSpace(colC) ? (string.IsNullOrWhiteSpace(colB) ? null : colB) : colC,
                            SoTinChi = soTinChi,
                            LoaiHocPhan = string.IsNullOrWhiteSpace(colD) ? null : colD,
                            DieuKienTienQuyet = string.IsNullOrWhiteSpace(colF) ? null : colF,
                            HocKiToChuc = hocKi
                        };

                        currentBlock.Value.Subjects.Add(mon);
                        if (currentBlock.Value.SumFromB && soTinChi.HasValue)
                        {
                            currentBlock = (currentBlock.Value.Block, currentBlock.Value.SumFromB, currentBlock.Value.Subjects);
                            currentBlock.Value.Block.TongTinChi = (currentBlock.Value.Block.TongTinChi ?? 0) + soTinChi.Value;
                        }

                        var detail = new ChiTietCtdt
                        {
                            Stt = sttCounter++,
                            MaHocPhan = mon.MaMon,
                            TenHocPhan = mon.TenMon,
                            SoTinChi = mon.SoTinChi,
                            LoaiHocPhan = mon.LoaiHocPhan,
                            DieuKienTienQuyet = mon.DieuKienTienQuyet,
                            HocKiToChuc = mon.HocKiToChuc
                        };

                        parsedChiTiet.Add(detail);
                        tongTinChi += mon.SoTinChi ?? 0;
                    }
                }
            }

            if (!parsedChiTiet.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy dữ liệu học phần trong file." });
            }

            if (!khoiImports.Any())
            {
                // Tạo khối mặc định nếu file chỉ chứa danh sách môn học
                var khoi = new KhoiKienThuc { TenKhoi = "Khối kiến thức chung", TongTinChi = tongTinChi };
                khoiImports.Add((khoi, false, new List<MonHoc>()));
            }

            // Xóa dữ liệu CTDT trùng (cùng khóa hoặc cùng mã) sau khi file hợp lệ
            var targetKhoaId = khoaHocId;
            var existingIds = await _context.ChuongTrinhDaoTaos
                .Where(ct => ct.MaCtdt == targetMaCtdt || (targetKhoaId.HasValue && ct.IdKhoaHoc == targetKhoaId.Value))
                .Select(ct => ct.Id)
                .ToListAsync();
            if (existingIds.Any())
            {
                var monOld = _context.MonHocs.Where(m => m.IdCtdt != null && existingIds.Contains(m.IdCtdt.Value));
                _context.MonHocs.RemoveRange(monOld);

                var khoiOld = _context.KhoiKienThucs.Where(k => k.IdCtdt != null && existingIds.Contains(k.IdCtdt.Value));
                _context.KhoiKienThucs.RemoveRange(khoiOld);

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
            await _context.SaveChangesAsync();

            foreach (var block in khoiImports)
            {
                block.Block.IdCtdt = newCtdt.Id;
            }

            _context.KhoiKienThucs.AddRange(khoiImports.Select(k => k.Block));
            await _context.SaveChangesAsync();

            var monToInsert = new List<MonHoc>();
            foreach (var block in khoiImports)
            {
                foreach (var mon in block.Subjects)
                {
                    mon.IdKhoiKienThuc = block.Block.Id;
                    mon.IdCtdt = newCtdt.Id;
                    if (string.IsNullOrWhiteSpace(mon.TenMon) && !string.IsNullOrWhiteSpace(mon.MaMon))
                    {
                        mon.TenMon = mon.MaMon;
                    }
                    monToInsert.Add(mon);
                }
            }

            if (monToInsert.Any())
            {
                _context.MonHocs.AddRange(monToInsert);
            }

            var computedTongTc = monToInsert.Sum(m => m.SoTinChi ?? 0);
            if (computedTongTc == 0)
            {
                computedTongTc = khoiImports.Sum(k => k.Block.TongTinChi ?? 0);
            }
            newCtdt.TongTinChi = computedTongTc;
            _context.ChuongTrinhDaoTaos.Update(newCtdt);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Đã import CTĐT thành công. Khối: {khoiImports.Count}, Môn: {monToInsert.Count}, Tổng tín chỉ: {newCtdt.TongTinChi}"
            });
        }
    }
}
