using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller xem nhật ký hướng dẫn cho sinh viên
    /// Kế thừa BaseSinhVienController để kiểm tra nguyện vọng đã duyệt
    /// </summary>
    public class NhatKyHuongDanController : BaseSinhVienController
    {
        public NhatKyHuongDanController(QuanLyDoAnTotNghiepContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Kiểm tra đợt đồ án
            var dot = await GetDotDoAnActive();
            if (dot == null)
            {
                ViewBag.ThongBao = "Hiện tại chưa có đợt đồ án nào đang hoạt động.";
                return View(new List<NhatKyHuongDan>().ToPagedList(1, pageSize));
            }

            ViewBag.TenDot = dot.TenDot;

            // Lấy thông tin sinh viên
            var sinhVien = await GetSinhVienHienTai();
            if (sinhVien == null)
            {
                ViewBag.ThongBao = "Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.";
                return View(new List<NhatKyHuongDan>().ToPagedList(1, pageSize));
            }

            // Kiểm tra SV đã có đề tài được GVHD duyệt
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            if (svDeTai == null)
            {
                ViewBag.ThongBao = "Bạn chưa được duyệt vào đề tài nào. Vui lòng chờ giảng viên duyệt đăng ký đề tài.";
                return View(new List<NhatKyHuongDan>().ToPagedList(1, pageSize));
            }

            // ============================================
            // BUSINESS RULE: Đề tài phải được HỘI ĐỒNG DUYỆT (TrangThai = DA_DUYET)
            // ============================================
            var deTai = svDeTai.IdDeTaiNavigation;
            if (deTai == null || deTai.TrangThai != "DA_DUYET")
            {
                ViewBag.ThongBao = "Đề tài của bạn chưa được hội đồng duyệt. Vui lòng chờ hội đồng xét duyệt đề tài.";
                ViewBag.MaDeTai = deTai?.MaDeTai;
                ViewBag.TenDeTai = deTai?.TenDeTai;
                ViewBag.TrangThaiDeTai = deTai?.TrangThai;
                return View(new List<NhatKyHuongDan>().ToPagedList(1, pageSize));
            }

            // Đủ điều kiện - load dữ liệu
            ViewBag.GiaiDoan = "DA_MO";
            ViewBag.MaDeTai = deTai.MaDeTai;
            ViewBag.TenDeTai = deTai.TenDeTai;

            var nhatKys = await _context.NhatKyHuongDans
                .Include(n => n.IdDotNavigation)
                .Where(n => n.IdDot == dot.Id)
                .OrderByDescending(n => n.NgayHop)
                .ToListAsync();

            return View(nhatKys.ToPagedList(pageNumber, pageSize));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] NhatKyHuongDanCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NgayHop) || string.IsNullOrWhiteSpace(dto.ThoiGianHop)
                || string.IsNullOrWhiteSpace(dto.HinhThucHop) || string.IsNullOrWhiteSpace(dto.MucTieuBuoiHop)
                || string.IsNullOrWhiteSpace(dto.NoiDungHop))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ các trường bắt buộc." });
            }

            // Kiểm tra đề tài đã được hội đồng duyệt chưa
            var sinhVien = await GetSinhVienHienTai();
            if (sinhVien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });
            }

            var dot = await GetDotDoAnActive();
            if (dot == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đợt đồ án." });
            }

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            if (svDeTai?.IdDeTaiNavigation?.TrangThai != "DA_DUYET")
            {
                return Json(new { success = false, message = "Đề tài chưa được hội đồng duyệt. Không thể thêm nhật ký." });
            }

            var nhatKy = new NhatKyHuongDan
            {
                IdDot = dot.Id,
                NgayHop = DateOnly.TryParse(dto.NgayHop, out var nh) ? nh : null,
                ThoiGianHop = TimeOnly.TryParse(dto.ThoiGianHop, out var th) ? th : null,
                HinhThucHop = dto.HinhThucHop,
                DiaDiemHop = dto.DiaDiemHop,
                ThanhVienThamDu = dto.ThanhVienThamDu,
                TenGvhd = dto.TenGvhd,
                MucTieuBuoiHop = dto.MucTieuBuoiHop,
                NoiDungHop = dto.NoiDungHop,
                ActionList = dto.ActionList
            };

            _context.NhatKyHuongDans.Add(nhatKy);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }

    public class NhatKyHuongDanCreateDto
    {
        public string? NgayHop { get; set; }
        public string? ThoiGianHop { get; set; }
        public string? HinhThucHop { get; set; }
        public string? DiaDiemHop { get; set; }
        public string? ThanhVienThamDu { get; set; }
        public string? TenGvhd { get; set; }
        public string? MucTieuBuoiHop { get; set; }
        public string? NoiDungHop { get; set; }
        public string? ActionList { get; set; }
    }
}
