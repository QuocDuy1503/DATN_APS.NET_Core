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
                .Include(c => c.KhoiKienThucs)
                .Include(c => c.ChiTietCtdts)
                    .ThenInclude(hp => hp.IdKhoiKienThucNavigation)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (ct == null) return NotFound();

            var khoiVMs = ct.KhoiKienThucs
                .OrderBy(k => k.Id)
                .Select(k => new CTDTKhoiKienThucViewModel
                {
                    Id = k.Id,
                    TenKhoi = k.TenKhoi ?? "",
                    TongTinChi = k.TongTinChi ?? 0,
                    HocPhans = ct.ChiTietCtdts
                        .Where(hp => hp.IdKhoiKienThuc == k.Id)
                        .OrderBy(hp => hp.Stt)
                        .Select(hp => new CTDTHocPhanViewModel
                        {
                            Stt = hp.Stt ?? 0,
                            MaHocPhan = hp.MaHocPhan ?? "",
                            TenHocPhan = hp.TenHocPhan ?? "",
                            SoTinChi = hp.SoTinChi ?? 0,
                            LoaiHocPhan = hp.LoaiHocPhan ?? "",
                            DieuKienTienQuyet = hp.DieuKienTienQuyet,
                            HocKiToChuc = hp.HocKiToChuc ?? 0,
                            KhoiKienThuc = k.TenKhoi ?? ""
                        }).ToList()
                }).ToList();

            // Các HP chưa gán khối
            var ungrouped = ct.ChiTietCtdts
                .Where(hp => hp.IdKhoiKienThuc == null)
                .OrderBy(hp => hp.Stt)
                .Select(hp => new CTDTHocPhanViewModel
                {
                    Stt = hp.Stt ?? 0,
                    MaHocPhan = hp.MaHocPhan ?? "",
                    TenHocPhan = hp.TenHocPhan ?? "",
                    SoTinChi = hp.SoTinChi ?? 0,
                    LoaiHocPhan = hp.LoaiHocPhan ?? "",
                    DieuKienTienQuyet = hp.DieuKienTienQuyet,
                    HocKiToChuc = hp.HocKiToChuc ?? 0,
                    KhoiKienThuc = ""
                }).ToList();

            if (ungrouped.Any())
            {
                khoiVMs.Add(new CTDTKhoiKienThucViewModel
                {
                    TenKhoi = "Chưa phân loại",
                    HocPhans = ungrouped
                });
            }

            var allHPs = khoiVMs.SelectMany(k => k.HocPhans).ToList();

            var detail = new CTDTDetailViewModel
            {
                Id = ct.Id,
                MaCtdt = ct.MaCtdt,
                TenCtdt = ct.TenCtdt ?? "",
                Khoa = ct.IdKhoaHocNavigation?.TenKhoa ?? "",
                TongTinChi = ct.TongTinChi ?? 0,
                KhoiKienThucs = khoiVMs,
                HocPhans = allHPs
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

            var khoiImports = new List<(KhoiKienThuc Block, List<ChiTietCtdt> Details, List<MonHoc> Subjects)>();
            (KhoiKienThuc Block, List<ChiTietCtdt> Details, List<MonHoc> Subjects)? currentBlock = null;
            int sttCounter = 1;

            int? SafeParseInt(string? input)
            {
                if (string.IsNullOrWhiteSpace(input)) return null;
                input = input.Trim();
                if (int.TryParse(input, out var val)) return val;
                var match = Regex.Match(input, @"-?\d+");
                if (match.Success && int.TryParse(match.Value, out var inner)) return inner;
                return null;
            }

            int SumAllNumbers(string? input)
            {
                if (string.IsNullOrWhiteSpace(input)) return 0;
                var matches = Regex.Matches(input, @"\d+");
                return matches.Sum(m => int.TryParse(m.Value, out var v) ? v : 0);
            }

            string RemoveParentheses(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                return Regex.Replace(value, @"\s*\(.*?\)\s*", string.Empty).Trim();
            }

            string StripLeadingNumber(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                return Regex.Replace(value, @"^\d+(\.\d+)*\.?\s*", string.Empty).Trim();
            }

            bool IsDottedNumber(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
                return Regex.IsMatch(value.Trim(), @"^\d+(\.\d+)+");
            }

            void StartNewBlock(string name, int credits)
            {
                var khoi = new KhoiKienThuc { TenKhoi = name, TongTinChi = credits };
                currentBlock = (khoi, new List<ChiTietCtdt>(), new List<MonHoc>());
                khoiImports.Add(currentBlock.Value);
            }

            void AddSubjectRow(string colB, string colC, string colE, string colI, string colJ, string colK)
            {
                if (currentBlock == null) return;

                var maHP = string.IsNullOrWhiteSpace(colB) ? null : colB;
                var tenHP = string.IsNullOrWhiteSpace(colC) ? maHP : colC;
                var soTC = SafeParseInt(colE);
                var loaiHP = string.IsNullOrWhiteSpace(colI) ? null : colI;
                var dkTQ = string.IsNullOrWhiteSpace(colJ) ? null : colJ;
                var hocKi = SafeParseInt(colK);

                if (string.IsNullOrWhiteSpace(tenHP)) return;

                var detail = new ChiTietCtdt
                {
                    Stt = sttCounter++,
                    MaHocPhan = maHP,
                    TenHocPhan = tenHP,
                    SoTinChi = soTC,
                    LoaiHocPhan = loaiHP,
                    DieuKienTienQuyet = dkTQ,
                    HocKiToChuc = hocKi
                };
                currentBlock.Value.Details.Add(detail);

                var mon = new MonHoc
                {
                    MaMon = maHP,
                    TenMon = tenHP,
                    SoTinChi = soTC,
                    LoaiHocPhan = loaiHP,
                    DieuKienTienQuyet = dkTQ,
                    HocKiToChuc = hocKi
                };
                currentBlock.Value.Subjects.Add(mon);
            }

            bool IsSubjectRow(string colB, string colC, string colE)
            {
                return !string.IsNullOrWhiteSpace(colB) && !string.IsNullOrWhiteSpace(colC)
                       && !string.IsNullOrWhiteSpace(colE);
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
                    int startRow = 6;
                    bool phase2 = false;
                    bool skipNextRow = false;

                    for (int row = startRow; row <= rowCount; row++)
                    {
                        if (skipNextRow) { skipNextRow = false; continue; }

                        var colA = worksheet.Cell(row, 1).GetValue<string>().Trim();
                        var colB = worksheet.Cell(row, 2).GetValue<string>().Trim();
                        var colC = worksheet.Cell(row, 3).GetValue<string>().Trim();
                        var colE = worksheet.Cell(row, 5).GetValue<string>().Trim();
                        var colI = worksheet.Cell(row, 9).GetValue<string>().Trim();
                        var colJ = worksheet.Cell(row, 10).GetValue<string>().Trim();
                        var colK = worksheet.Cell(row, 11).GetValue<string>().Trim();

                        // Điểm dừng
                        if (colA.Contains("Không lựa chọn chuyên ngành", StringComparison.OrdinalIgnoreCase))
                            break;

                        // Kiểm tra chuyển sang giai đoạn 2 (chỉ dùng làm cột mốc, không tạo khối)
                        if (!phase2 && colB.Contains("Kiến thức giáo dục chuyên nghiệp", StringComparison.OrdinalIgnoreCase))
                        {
                            phase2 = true;
                            continue;
                        }

                        if (!phase2)
                        {
                            // === GIAI ĐOẠN 1 ===
                            // Dòng đầu tiên (row 6) hoặc dòng khối: B có dữ liệu, E có dữ liệu, C rỗng
                            bool isBlockHeader = !string.IsNullOrWhiteSpace(colB)
                                                 && !string.IsNullOrWhiteSpace(colE)
                                                 && string.IsNullOrWhiteSpace(colC);

                            if (isBlockHeader)
                            {
                                StartNewBlock(RemoveParentheses(colB), SafeParseInt(colE) ?? 0);
                                continue;
                            }

                            // Dòng học phần
                            if (IsSubjectRow(colB, colC, colE))
                            {
                                if (currentBlock == null)
                                    StartNewBlock("Khối kiến thức chung", 0);
                                AddSubjectRow(colB, colC, colE, colI, colJ, colK);
                            }
                     
                        }
                        else
                        {
                            // === GIAI ĐOẠN 2 ===
                            bool colAHasDottedNum = IsDottedNumber(colA);

                            if (colAHasDottedNum)
                            {
                                bool bHasData = !string.IsNullOrWhiteSpace(colB);
                                bool eHasData = !string.IsNullOrWhiteSpace(colE);

                                if (bHasData && eHasData)
                                {
                                    // Trường hợp A: tên khối ở B, TC ở E
                                    StartNewBlock(RemoveParentheses(colB), SafeParseInt(colE) ?? 0);
                                }
                                else if (!bHasData && !eHasData)
                                {
                                    // Trường hợp B: tên khối nằm trong A, TC ở dòng kế tiếp cột B
                                    var blockName = StripLeadingNumber(colA);
                                    int blockTC = 0;
                                    if (row + 1 <= rowCount)
                                    {
                                        var nextColB = worksheet.Cell(row + 1, 2).GetValue<string>().Trim();
                                        blockTC = SumAllNumbers(nextColB);
                                        skipNextRow = true;
                                    }
                                    StartNewBlock(blockName, blockTC);
                                }
                                else if (bHasData && !eHasData)
                                {
                                    // Có số dấu chấm ở A, B có data nhưng E rỗng — có thể là khối không ghi TC
                                    StartNewBlock(RemoveParentheses(colB), 0);
                                }
                                continue;
                            }

                            // Dòng học phần
                            if (IsSubjectRow(colB, colC, colE))
                            {
                                AddSubjectRow(colB, colC, colE, colI, colJ, colK);
                            }
                          
                        }
                    }
                }
            }

            var allDetails = khoiImports.SelectMany(k => k.Details).ToList();
            if (!allDetails.Any())
                return Json(new { success = false, message = "Không tìm thấy dữ liệu học phần trong file." });

            // Xóa dữ liệu CTDT trùng
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
                var detailsOld = _context.ChiTietCtdts.Where(d => d.IdCtdt != null && existingIds.Contains(d.IdCtdt.Value));
                _context.ChiTietCtdts.RemoveRange(detailsOld);
                var programs = _context.ChuongTrinhDaoTaos.Where(ct => existingIds.Contains(ct.Id));
                _context.ChuongTrinhDaoTaos.RemoveRange(programs);
                await _context.SaveChangesAsync();
            }

            // Tạo CTDT mới
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

            // Lưu khối kiến thức
            foreach (var block in khoiImports)
                block.Block.IdCtdt = newCtdt.Id;
            _context.KhoiKienThucs.AddRange(khoiImports.Select(k => k.Block));
            await _context.SaveChangesAsync();

            // Lưu chi tiết CTDT (có gán IdKhoiKienThuc)
            foreach (var block in khoiImports)
            {
                foreach (var ct in block.Details)
                {
                    ct.IdCtdt = newCtdt.Id;
                    ct.IdKhoiKienThuc = block.Block.Id;
                }
            }
            _context.ChiTietCtdts.AddRange(allDetails);

            // Lưu môn học
            var monToInsert = new List<MonHoc>();
            foreach (var block in khoiImports)
            {
                foreach (var mon in block.Subjects)
                {
                    mon.IdKhoiKienThuc = block.Block.Id;
                    mon.IdCtdt = newCtdt.Id;
                    monToInsert.Add(mon);
                }
            }
            if (monToInsert.Any())
                _context.MonHocs.AddRange(monToInsert);

            // Tính tổng tín chỉ
            var computedTongTc = khoiImports.Sum(k => k.Block.TongTinChi ?? 0);
            if (computedTongTc == 0)
                computedTongTc = monToInsert.Sum(m => m.SoTinChi ?? 0);
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
