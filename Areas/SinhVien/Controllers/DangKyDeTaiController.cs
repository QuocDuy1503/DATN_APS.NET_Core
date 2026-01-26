using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    public class DangKyDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DangKyDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }


        

        // GET: Danh sách đề tài để SV đăng ký
        [HttpGet]
        public async Task<IActionResult> Index(DangKyDeTaiViewModel model)
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(x => x.Mssv == mssv);

            if (sinhVien == null)
            {
                ViewBag.Message = "Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.";
                return View(new List<DangKyDeTaiViewModel>());
            }

            // Check đợt đăng ký hiện tại

            var dotHienTai = await GetDotDangKyHienTai();

            if (dotHienTai == null)
            {
                // Trả view rỗng + có message (tuỳ bạn)
                ViewBag.Message = "Chưa đến giai đoạn đăng ký đề tài hoặc đã hết hạn.";
                return View(new List<DangKyDeTaiViewModel>());
            }

            // Đề tài SV đã đăng ký (để disable/hiển thị trạng thái)
            var daDangKyIdDeTai = await _context.SinhVienDeTais
                .Where(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTaiNavigation.IdDot == dotHienTai.Id)
                .Select(x => x.IdDeTai)
                .ToListAsync();

            var detaiList = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv.IdNguoiDungNavigation)
                .Where(dt => dt.IdDot == dotHienTai.Id)
                .OrderByDescending(dt => dt.Id)
                .ToListAsync();

            var data = detaiList.Select(dt => new DangKyDeTaiViewModel
            {
                IdDeTai = dt.Id,
                MaDeTai = dt.MaDeTai,
                TenDeTai = dt.TenDeTai,
                Nganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                GVHD = dt.IdGvhdNavigation != null && dt.IdGvhdNavigation.IdNguoiDungNavigation != null
                    ? dt.IdGvhdNavigation.IdNguoiDungNavigation.HoTen
                    : "",
                TrangThai = dt.TrangThai,
                StatusCss = MapTrangThaiCss(dt.TrangThai),
                DaDangKy = daDangKyIdDeTai.Contains(dt.Id)
            }).ToList();

            return View(data);
        }
        // POST: Sinh viên đăng ký đề tài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(int idDeTai)
        {
            var dot = await GetDotDangKyHienTai();
            if (dot == null)
            {
                return Json(new { success = false, message = "Chưa đến giai đoạn đăng ký hoặc đã hết hạn." });
            }

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(x => x.Mssv == mssv);

            if (sinhVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });

            var deTai = await _context.DeTais.FirstOrDefaultAsync(dt => dt.Id == idDeTai && dt.IdDot == dot.Id);
            if (deTai == null)
                return Json(new { success = false, message = "Đề tài không tồn tại hoặc không thuộc đợt hiện tại." });

            // Chặn đăng ký trùng
            var daDangKy = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTai == idDeTai);
            if (daDangKy)
                return Json(new { success = false, message = "Bạn đã đăng ký đề tài này rồi." });

            // Nếu quy định: 1 SV chỉ được đăng ký 1 đề tài trong đợt
            var daDangKyDeTaiKhac = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTaiNavigation.IdDot == dot.Id);
            if (daDangKyDeTaiKhac)
                return Json(new { success = false, message = "Bạn đã đăng ký 1 đề tài khác trong đợt này." });

            // Nếu đề tài đã khóa đăng ký / full slot / không cho đăng ký
            if (deTai.TrangThai == "Đã khóa đăng ký")
                return Json(new { success = false, message = "Đề tài đã khóa đăng ký." });

            try
            {
                // insert bảng trung gian
                var svdt = new SinhVienDeTai
                {
                    IdDeTai = deTai.Id,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    TrangThai = "Chờ GVHD duyệt",
                    NgayDangKy = DateTime.Now
                };

                _context.SinhVienDeTais.Add(svdt);

                // cập nhật trạng thái đề tài (tuỳ quy trình)
                // ví dụ: có SV đăng ký
                if (deTai.TrangThai == "Chưa có SV đăng ký")
                    deTai.TrangThai = "Có SV đăng ký";

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đăng ký đề tài thành công! Vui lòng chờ giảng viên duyệt." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }

        // GET: Chi tiết đề tài (để mở modal giống ChiTiet của bạn)
        [HttpGet]
        public async Task<IActionResult> XCT_DT(int id)
        {
            var deTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id);

            if (deTai == null) return NotFound();

            return Json(new
            {
                success = true,
                data = new
                {
                    deTai.Id,
                    deTai.MaDeTai,
                    deTai.TenDeTai,
                    TenChuyenNganh = deTai.IdChuyenNganhNavigation?.TenChuyenNganh,
                    GVHD = deTai.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen,
                    deTai.TrangThai,
                    deTai.MucTieuChinh,
                    deTai.PhamViChucNang,
                    deTai.CongNgheSuDung
                }
            });
        }

        // API check trạng thái mở cổng đăng ký
        [HttpGet]
        public async Task<IActionResult> KiemTraDangKy()
        {
            var dot = await GetDotDangKyHienTai();
            if (dot == null)
            {
                return Json(new { dangMo = false, message = "Hiện tại chưa mở cổng đăng ký đề tài." });
            }

            return Json(new
            {
                dangMo = true,
                tenDot = dot.TenDot,
                ngayBatDau = dot.NgayBatDauDeXuatDeTai,
                ngayKetThuc = dot.NgayKetThucDeXuatDeTai
            });
        }

        // ================= PRIVATE =================

        private string MapTrangThaiCss(string trangThai)
        {
            return trangThai switch
            {
                "Chưa có SV đăng ký" => "badge-gray",
                "Có SV đăng ký" => "badge-blue",
                "Đã khóa đăng ký" => "badge-red",
                "Đang thực hiện" => "badge-orange",
                "Hoàn thành" => "badge-green",
                _ => "badge-dark"
            };
        }

        private async Task<DotDoAn?> GetDotDangKyHienTai()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            return await _context.DotDoAns
                .Where(d => d.TrangThai == true
                    && d.NgayBatDauDeXuatDeTai <= today
                    && d.NgayKetThucDeXuatDeTai >= today)
                .FirstOrDefaultAsync();
        }
    }
}
