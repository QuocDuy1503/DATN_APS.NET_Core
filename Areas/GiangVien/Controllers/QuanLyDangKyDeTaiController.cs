using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLyDangKyDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDangKyDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isLecturerByClaim = User?.Identity?.IsAuthenticated == true && (User.IsInRole("GIANG_VIEN") || User.IsInRole("GV"));
            var isLecturerBySession = sessionRole == "GIANG_VIEN" || sessionRole == "GV";

            if (!isLecturerByClaim && !isLecturerBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
                return View(new List<DangKyDeTaiGVItem>());

            var data = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdKhoaHocNavigation)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung)
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new DangKyDeTaiGVItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    ChuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    SoLuongDangKy = dt.SinhVienDeTais.Count,
                    DanhSachSV = dt.SinhVienDeTais.Select(svdt => new SinhVienDangKyItem
                    {
                        IdSvDeTai = svdt.Id,
                        Mssv = svdt.IdSinhVienNavigation != null ? svdt.IdSinhVienNavigation.Mssv : "",
                        HoTen = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                            ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : "",
                        KhoaHoc = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdKhoaHocNavigation != null
                            ? svdt.IdSinhVienNavigation.IdKhoaHocNavigation.TenKhoa : "",
                        TrangThai = svdt.TrangThai,
                        NgayDangKy = svdt.NgayDangKy.HasValue ? svdt.NgayDangKy.Value.ToString("dd/MM/yyyy") : ""
                    }).ToList()
                })
                .ToListAsync();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetSinhVien([FromBody] DuyetSvRequest request)
        {
            var svDeTai = await _context.SinhVienDeTais.FindAsync(request.IdSvDeTai);
            if (svDeTai == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi đăng ký." });

            var deTai = await _context.DeTais.FindAsync(svDeTai.IdDeTai);
            if (deTai == null)
                return Json(new { success = false, message = "Không tìm thấy đề tài." });

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền duyệt sinh viên cho đề tài này." });

            if (request.Action == "approve")
            {
                var soSvDaDuyet = await _context.SinhVienDeTais
                    .CountAsync(s => s.IdDeTai == deTai.Id && s.TrangThai == "DA_DUYET");
                if (soSvDaDuyet >= 2)
                    return Json(new { success = false, message = "Đề tài đã đủ số lượng sinh viên (tối đa 2)." });

                svDeTai.TrangThai = "DA_DUYET";
            }
            else if (request.Action == "reject")
            {
                svDeTai.TrangThai = "TU_CHOI";
                svDeTai.NhanXet = request.NhanXet;
            }
            else
            {
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = request.Action == "approve" ? "Duyệt thành công!" : "Đã từ chối." });
        }
    }

    public class DuyetSvRequest
    {
        public int IdSvDeTai { get; set; }
        public string Action { get; set; } = "";
        public string? NhanXet { get; set; }
    }
}
