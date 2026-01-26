using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Areas.GV_BoMon.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GV_BoMon.Controllers
{
    [Area("GV_BoMon")]
    public class QuanLyHoiDongDuyetDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyHoiDongDuyetDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // --- DANH SÁCH HỘI ĐỒNG ---
        public IActionResult Index(int? page, int? dotId, int? namHoc, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // 1. Load Dropdown cho Filter và Modal
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);

            var listNamHoc = _context.HocKis
                .Select(h => new { h.NamBatDau, TenNam = $"{h.NamBatDau}-{h.NamKetThuc}" })
                .Distinct()
                .OrderByDescending(n => n.NamBatDau)
                .ToList();
            ViewBag.ListNamHoc = new SelectList(listNamHoc, "NamBatDau", "TenNam", namHoc);

            // Dữ liệu cho Modal Thêm mới (Dropdown Bộ môn)
            ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon");

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentNamHoc = namHoc;
            ViewBag.CurrentFilter = searchString;

            // 2. Query dữ liệu từ SQL
            var query = _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdNguoiTaoNavigation)
                .Include(hd => hd.IdDotNavigation)
                .ThenInclude(d => d.IdHocKiNavigation)
                .Where(hd => hd.LoaiHoiDong == "DUYET_DE_TAI" || hd.LoaiHoiDong == "CUOI_KY" || hd.LoaiHoiDong == null)
                .AsQueryable();

            // 3. Áp dụng bộ lọc
            if (dotId.HasValue)
            {
                query = query.Where(hd => hd.IdDot == dotId);
            }
            if (namHoc.HasValue)
            {
                query = query.Where(hd => hd.IdDotNavigation.IdHocKiNavigation.NamBatDau == namHoc);
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(hd => hd.MaHoiDong.Contains(searchString) || hd.TenHoiDong.Contains(searchString));
            }

            // 4. Map sang ViewModel
            var modelQuery = query.Select(hd => new GV_BoMon.Models.QuanLyHoiDongViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong,
                TenHoiDong = hd.TenHoiDong,
                TenBoMon = hd.IdBoMonNavigation != null ? hd.IdBoMonNavigation.TenBoMon : "",
                NguoiTao = hd.IdNguoiTaoNavigation != null ? hd.IdNguoiTaoNavigation.HoTen : "",
                IdBoMon = hd.IdBoMon ?? 0,

                NgayBatDau = hd.NgayBatDau.HasValue
                             ? hd.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                             : (hd.NgayBaoCao.HasValue ? hd.NgayBaoCao.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null),

                NgayKetThuc = hd.NgayKetThuc.HasValue
                              ? hd.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                              : (DateTime?)null
            });

            var pagedList = modelQuery.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // --- TẠO MỚI HỘI ĐỒNG ---
        [HttpPost]
        public async Task<IActionResult> Create(QuanLyHoiDongViewModel model)
        {
            try
            {
                // Lấy IdNguoiTao từ session
                int idNguoiTao = 1;
                var userEmail = HttpContext.Session.GetString("UserEmail");
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var nguoiDung = await _context.NguoiDungs
                        .FirstOrDefaultAsync(nd => nd.Email == userEmail);
                    if (nguoiDung != null) idNguoiTao = nguoiDung.Id;
                }

                var hoidong = new HoiDongBaoCao
                {
                    MaHoiDong = model.MaHoiDong,
                    TenHoiDong = model.TenHoiDong,
                    IdBoMon = model.IdBoMon,
                    LoaiHoiDong = "DUYET_DE_TAI",
                    NgayBatDau = model.NgayBatDau.HasValue ? DateOnly.FromDateTime(model.NgayBatDau.Value) : null,
                    NgayKetThuc = model.NgayKetThuc.HasValue ? DateOnly.FromDateTime(model.NgayKetThuc.Value) : null,
                    NgayBaoCao = model.NgayBatDau.HasValue ? DateOnly.FromDateTime(model.NgayBatDau.Value) : null,
                    IdNguoiTao = idNguoiTao,
                };

                _context.Add(hoidong);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo hội đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // --- CHI TIẾT / CHỈNH SỬA HỘI ĐỒNG ---
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hd = await _context.HoiDongBaoCaos
                .Include(h => h.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hd == null) return NotFound();

            var model = new GV_BoMon.Models.QuanLyHoiDongEditViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong ?? "",
                TenHoiDong = hd.TenHoiDong ?? "",
                IdBoMon = hd.IdBoMon ?? 0,
                NgayBatDau = hd.NgayBatDau,
                NgayKetThuc = hd.NgayKetThuc,
                TrangThai = hd.TrangThai ?? false,

                // Map danh sách thành viên
                ThanhViens = hd.ThanhVienHdBaoCaos.Select(tv => new GV_BoMon.Models.ThanhVienHoiDongViewModel
                {
                    IdNguoiDung = tv.IdGiangVien ?? 0,
                    TenGiangVien = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "Chưa cập nhật",
                    Email = tv.IdGiangVienNavigation?.IdNguoiDungNavigation?.Email ?? "",
                    MaGV = tv.IdGiangVienNavigation?.MaGv ?? "",
                    HocVi = tv.IdGiangVienNavigation?.HocVi ?? "",
                    VaiTro = tv.VaiTro ?? ""
                }).ToList()
            };

            ViewBag.ListBoMon = new SelectList(_context.BoMons.ToList(), "Id", "TenBoMon", model.IdBoMon);
            return View(model);
        }

        // POST: Lưu thay đổi thông tin chung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuanLyHoiDongEditViewModel model)
        {
            var hd = await _context.HoiDongBaoCaos.FindAsync(model.Id);
            if (hd == null) return NotFound();

            hd.TenHoiDong = model.TenHoiDong;
            hd.IdBoMon = model.IdBoMon;
            hd.NgayBatDau = model.NgayBatDau;
            hd.NgayKetThuc = model.NgayKetThuc;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã lưu thông tin hội đồng thành công!";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        // --- API: TÌM KIẾM GIẢNG VIÊN ---
        [HttpGet]
        public IActionResult SearchGiangVien(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var data = _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .Where(gv => gv.IdNguoiDungNavigation != null &&
                       (gv.IdNguoiDungNavigation.HoTen.Contains(term) ||
                        gv.MaGv.Contains(term) ||
                        gv.IdNguoiDungNavigation.Email.Contains(term)))
                .Select(gv => new
                {
                    id = gv.IdNguoiDung,
                    label = $"{gv.IdNguoiDungNavigation.HoTen} ({gv.MaGv})",
                    email = gv.IdNguoiDungNavigation.Email ?? "",
                    maGv = gv.MaGv ?? "",
                    hoTen = gv.IdNguoiDungNavigation.HoTen ?? "",
                    hocVi = gv.HocVi ?? ""
                })
                .Take(10)
                .ToList();

            return Json(data);
        }

        // --- API: THÊM THÀNH VIÊN VÀO HỘI ĐỒNG ---
        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] AddMemberRequest req)
        {
            try
            {
                if (req == null || req.CouncilId <= 0 || req.IdGiangVien <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });
                }

                // Kiểm tra hội đồng tồn tại
                var hoiDong = await _context.HoiDongBaoCaos.FindAsync(req.CouncilId);
                if (hoiDong == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hội đồng!" });
                }

                // Kiểm tra giảng viên đã có trong hội đồng chưa
                var exists = await _context.ThanhVienHdBaoCaos
                    .AnyAsync(x => x.IdHdBaocao == req.CouncilId && x.IdGiangVien == req.IdGiangVien);
                if (exists)
                {
                    return Json(new { success = false, message = "Giảng viên này đã có trong hội đồng!" });
                }

                // Kiểm tra logic Chủ tịch
                if (req.VaiTro == "CHU_TICH")
                {
                    var hasPresident = await _context.ThanhVienHdBaoCaos
                        .AnyAsync(x => x.IdHdBaocao == req.CouncilId && x.VaiTro == "CHU_TICH");
                    if (hasPresident)
                    {
                        return Json(new { success = false, message = "Hội đồng này đã có Chủ tịch rồi!" });
                    }

                    // Kiểm tra học vị
                    var giangVien = await _context.GiangViens.FindAsync(req.IdGiangVien);
                    if (giangVien != null)
                    {
                        string hocVi = giangVien.HocVi?.ToLower() ?? "";
                        bool duDieuKien = hocVi.Contains("tiến sĩ") || hocVi.Contains("ts") ||
                                         hocVi.Contains("phó giáo sư") || hocVi.Contains("pgs") ||
                                         hocVi.Contains("giáo sư") || hocVi.Contains("gs");
                        if (!duDieuKien)
                        {
                            return Json(new { success = false, message = $"Giảng viên này có học vị '{giangVien.HocVi}'. Chủ tịch phải có học vị Tiến sĩ trở lên!" });
                        }
                    }
                }

                // Thêm thành viên
                var tv = new ThanhVienHdBaoCao
                {
                    IdHdBaocao = req.CouncilId,
                    IdGiangVien = req.IdGiangVien,
                    VaiTro = req.VaiTro
                };

                _context.ThanhVienHdBaoCaos.Add(tv);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm thành viên thành công!" });
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    msg = inner.Message;
                    inner = inner.InnerException;
                }
                return Json(new { success = false, message = "Lỗi: " + msg });
            }
        }

        // --- API: XÓA THÀNH VIÊN KHỎI HỘI ĐỒNG ---
        [HttpPost]
        public async Task<IActionResult> RemoveMember([FromBody] RemoveMemberRequest req)
        {
            try
            {
                var tv = await _context.ThanhVienHdBaoCaos
                    .FirstOrDefaultAsync(x => x.IdHdBaocao == req.CouncilId && x.IdGiangVien == req.IdGiangVien);

                if (tv == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thành viên này trong hội đồng!" });
                }

                _context.ThanhVienHdBaoCaos.Remove(tv);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa thành viên khỏi hội đồng!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // --- XÓA HỘI ĐỒNG ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hd = await _context.HoiDongBaoCaos
                    .Include(h => h.ThanhVienHdBaoCaos)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (hd == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hội đồng!";
                    return RedirectToAction(nameof(Index));
                }

                // Xóa thành viên trước
                if (hd.ThanhVienHdBaoCaos.Any())
                {
                    _context.ThanhVienHdBaoCaos.RemoveRange(hd.ThanhVienHdBaoCaos);
                }

                _context.HoiDongBaoCaos.Remove(hd);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa hội đồng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }

    // Request models
    public class AddMemberRequest
    {
        public int CouncilId { get; set; }
        public int IdGiangVien { get; set; }
        public string VaiTro { get; set; } = "UY_VIEN";
    }

    public class RemoveMemberRequest
    {
        public int CouncilId { get; set; }
        public int IdGiangVien { get; set; }
    }
}