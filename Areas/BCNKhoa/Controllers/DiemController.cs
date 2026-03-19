using System.IO;
using ClosedXML.Excel;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            var ctdtList = _context.ChuongTrinhDaoTaos
                .Include(c => c.IdKhoaHocNavigation)
                .Where(c => c.TrangThai == true)
                .OrderByDescending(c => c.IdKhoaHocNavigation != null ? c.IdKhoaHocNavigation.NamNhapHoc : 0)
                .ThenBy(c => c.SttHienThi)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = !string.IsNullOrWhiteSpace(c.TenCtdt)
                        ? c.TenCtdt
                        : c.MaCtdt
                })
                .ToList();

            ctdtList.Insert(0, new SelectListItem { Text = "-- Chọn chương trình đào tạo --", Value = "" });

            ViewBag.CtdtList = ctdtList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile? file, int? idCtdt)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file điểm.";
                return RedirectToAction(nameof(Index));
            }

            if (!idCtdt.HasValue || idCtdt <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn chương trình đào tạo trước khi import.";
                return RedirectToAction(nameof(Index));
            }

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (ext != ".xls" && ext != ".xlsx")
            {
                TempData["ErrorMessage"] = "Định dạng file không hợp lệ. Chỉ chấp nhận .xls, .xlsx.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Tải chi tiết CTDT kèm khối kiến thức để đối chiếu
                var chiTietCtdtList = await _context.ChiTietCtdts
                    .Include(ct => ct.IdKhoiKienThucNavigation)
                    .Where(ct => ct.IdCtdt == idCtdt.Value)
                    .ToListAsync();

                var courseCtdtMap = new Dictionary<string, (string TenKhoi, string LoaiHP)>(StringComparer.OrdinalIgnoreCase);
                foreach (var ct in chiTietCtdtList)
                {
                    string code = ct.MaHocPhan?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(code) && !courseCtdtMap.ContainsKey(code))
                    {
                        courseCtdtMap[code] = (
                            ct.IdKhoiKienThucNavigation?.TenKhoi ?? "",
                            ct.LoaiHocPhan ?? ""
                        );
                    }
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheets.First();

                int tongSV = 0, tongMH = 0;
                var errors = new List<string>();
                var warnings = new List<string>();
                var studentsData = new List<(int IdSV, string Mssv, List<KetQuaHocTap> Grades)>();

                int currentSvId = -1;
                string currentMssv = "";
                var currentGrades = new List<KetQuaHocTap>();
                int stt = 0;
                int emptyCount = 0;
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? 2;

                for (int row = 3; row <= lastRow + 5; row++)
                {
                    string a = CellStr(ws, row, 1);
                    string b = CellStr(ws, row, 2);
                    string c = CellStr(ws, row, 3);
                    string d = CellStr(ws, row, 4);
                    string e = CellStr(ws, row, 5);
                    string f = CellStr(ws, row, 6);
                    string g = CellStr(ws, row, 7);

                    bool allEmpty = string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)
                                 && string.IsNullOrEmpty(c) && string.IsNullOrEmpty(d)
                                 && string.IsNullOrEmpty(e) && string.IsNullOrEmpty(f)
                                 && string.IsNullOrEmpty(g);

                    if (allEmpty)
                    {
                        emptyCount++;
                        if (emptyCount >= 5)
                        {
                            FlushStudent(ref currentSvId, ref currentMssv, currentGrades, studentsData);
                            break;
                        }
                        continue;
                    }

                    emptyCount = 0;

                    bool isSvRow = !string.IsNullOrEmpty(a)
                                 && string.IsNullOrEmpty(b) && string.IsNullOrEmpty(c)
                                 && string.IsNullOrEmpty(d) && string.IsNullOrEmpty(e);

                    if (isSvRow)
                    {
                        FlushStudent(ref currentSvId, ref currentMssv, currentGrades, studentsData);
                        stt = 0;

                        string mssv = ExtractMssv(a);
                        if (!string.IsNullOrEmpty(mssv))
                        {
                            var sv = await _context.SinhViens
                                .FirstOrDefaultAsync(s => s.Mssv == mssv);

                            if (sv != null)
                            {
                                currentSvId = sv.IdNguoiDung;
                                currentMssv = mssv;
                            }
                            else
                            {
                                currentSvId = -1;
                                errors.Add("Dòng " + row + ": Không tìm thấy sinh viên MSSV '" + mssv + "'");
                            }
                        }
                    }
                    else if (currentSvId > 0 && !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b))
                    {
                        stt++;
                        double.TryParse(c, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double soTc);
                        double.TryParse(d, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double diemSo);
                        double.TryParse(f, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double gpa);

                        if (courseCtdtMap.Count > 0 && !courseCtdtMap.ContainsKey(a))
                        {
                            warnings.Add("SV " + currentMssv + " - Mã HP '" + a + "' không có trong CTDT đã chọn");
                        }

                        currentGrades.Add(new KetQuaHocTap
                        {
                            IdSinhVien = currentSvId,
                            Stt = stt,
                            MaHocPhan = a,
                            TenHocPhan = b,
                            SoTc = soTc,
                            DiemSo = diemSo,
                            DiemChu = e,
                            Gpa = gpa,
                            KetQua = ParseKetQua(g),
                            TongSoTinChi = 0
                        });
                    }
                }

                FlushStudent(ref currentSvId, ref currentMssv, currentGrades, studentsData);

                foreach (var (idSv, mssv, grades) in studentsData)
                {
                    var oldRecords = await _context.KetQuaHocTaps
                        .Where(k => k.IdSinhVien == idSv).ToListAsync();
                    _context.KetQuaHocTaps.RemoveRange(oldRecords);

                    double totalCredits = grades
                        .Where(x => x.KetQua == true)
                        .Sum(x => x.SoTc ?? 0);

                    foreach (var gr in grades)
                        gr.TongSoTinChi = totalCredits;

                    _context.KetQuaHocTaps.AddRange(grades);

                    var svEntity = await _context.SinhViens.FindAsync(idSv);
                    if (svEntity != null)
                        svEntity.TinChiTichLuy = totalCredits;

                    tongSV++;
                    tongMH += grades.Count;
                }

                await _context.SaveChangesAsync();

                if (tongSV > 0)
                    TempData["SuccessMessage"] = "Import thành công! Đã xử lý " + tongSV + " sinh viên, " + tongMH + " môn học.";
                else
                    TempData["ErrorMessage"] = "Không tìm thấy dữ liệu sinh viên hợp lệ trong file.";

                if (errors.Any())
                    TempData["WarningMessage"] = string.Join("\n", errors);

                if (warnings.Any())
                    TempData["WarningCtdt"] = string.Join("\n", warnings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi import: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helpers

        private static string CellStr(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            return cell.IsEmpty() ? "" : cell.GetString().Trim();
        }

        private static string ExtractMssv(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "";
            return value.Split(new[] { ' ', '-', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault()?.Trim() ?? "";
        }

        private static bool ParseKetQua(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string v = value.Trim().ToLower();
            return v == "đạt" || v == "dat" || v == "pass" || v == "1" || v == "true";
        }

        private static void FlushStudent(ref int svId, ref string mssv,
            List<KetQuaHocTap> grades, List<(int, string, List<KetQuaHocTap>)> data)
        {
            if (svId > 0 && grades.Count > 0)
                data.Add((svId, mssv, new List<KetQuaHocTap>(grades)));
            grades.Clear();
            svId = -1;
            mssv = "";
        }

        #endregion
    }
}