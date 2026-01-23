using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
//using DATN_TMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    public class DangKyNguyenVongController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DangKyNguyenVongController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // Kiểm tra đăng nhập trước mỗi action
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (HttpContext.Session.GetString("UserEmail") == null ||
                HttpContext.Session.GetString("Role") != "SV")
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        // GET: Hiển thị form đăng ký
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DangKyNguyenVongViewModel();

            // Lấy danh sách chuyên ngành cho dropdown
            viewModel.DanhSachChuyenNganh = await _context.ChuyenNganhs
                .Select(cn => new ChuyenNganhItem
                {
                    Id = cn.Id,
                    TenChuyenNganh = cn.TenChuyenNganh
                })
                .ToListAsync();

            // Tự động điền thông tin sinh viên đang đăng nhập
            var mssv = HttpContext.Session.GetString("UserCode");
            if (!string.IsNullOrEmpty(mssv))
            {
                var sinhVien = await _context.SinhViens
                    .Include(sv => sv.IdNguoiDungNavigation)
                    .Include(sv => sv.IdChuyenNganhNavigation)
                    .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

                if (sinhVien != null)
                {
                    viewModel.IdSinhVien = sinhVien.IdNguoiDung;
                    viewModel.Mssv = sinhVien.Mssv;
                    viewModel.HoTen = sinhVien.IdNguoiDungNavigation?.HoTen;
                    viewModel.Email = sinhVien.IdNguoiDungNavigation?.Email;
                    viewModel.Sdt = sinhVien.IdNguoiDungNavigation?.Sdt;
                    viewModel.IdChuyenNganh = sinhVien.IdChuyenNganh;
                    viewModel.TenChuyenNganh = sinhVien.IdChuyenNganhNavigation?.TenChuyenNganh;
                    viewModel.SoTinChiTichLuy = sinhVien.TinChiTichLuy;

                    // Lấy GPA
                    viewModel.Gpa = await TinhGPA(sinhVien.IdNguoiDung);
                }
            }

            // Lấy đợt đăng ký hiện tại
            var dotHienTai = await GetDotDangKyHienTai();
            if (dotHienTai != null)
            {
                viewModel.IdDot = dotHienTai.Id;
                viewModel.TenDot = dotHienTai.TenDot;
            }

            return View(viewModel);
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DangKyNguyenVongViewModel model)
        {
            // Kiểm tra đợt đăng ký trước
            var dotDoAn = await GetDotDangKyHienTai();
            if (dotDoAn == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Chưa đến giai đoạn đăng ký nguyện vọng hoặc đã hết hạn đăng ký. Vui lòng liên hệ phòng đào tạo để biết thêm chi tiết."
                });
            }

            // Lấy thông tin sinh viên
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });
            }

            // Kiểm tra đã đăng ký chưa
            var daDangKy = await _context.DangKyNguyenVongs
                .AnyAsync(dk => dk.IdSinhVien == sinhVien.IdNguoiDung && dk.IdDot == dotDoAn.Id);

            if (daDangKy)
            {
                return Json(new { success = false, message = "Bạn đã đăng ký nguyện vọng cho đợt này rồi." });
            }

            try
            {
                // Tạo bản ghi đăng ký mới
                var dangKy = new DangKyNguyenVong
                {
                    IdDot = dotDoAn.Id,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    SoTinChiTichLuyHienTai = (int?)model.SoTinChiTichLuy,
                    TrangThai = 0, // 0: Chờ xét duyệt
                    NgayDangKy = DateTime.Now
                };

                // Cập nhật thông tin sinh viên
                sinhVien.TinChiTichLuy = model.SoTinChiTichLuy;
                if (model.IdChuyenNganh.HasValue)
                {
                    sinhVien.IdChuyenNganh = model.IdChuyenNganh;
                }

                // Cập nhật SĐT
                if (sinhVien.IdNguoiDungNavigation != null && !string.IsNullOrEmpty(model.Sdt))
                {
                    sinhVien.IdNguoiDungNavigation.Sdt = model.Sdt;
                }

                _context.DangKyNguyenVongs.Add(dangKy);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đăng ký nguyện vọng đồ án thành công! Vui lòng chờ xét duyệt từ khoa/giảng viên hướng dẫn."
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau." });
            }
        }

        #region Private Methods

        // Lấy đợt đang trong giai đoạn đăng ký nguyện vọng
        private async Task<DotDoAn?> GetDotDangKyHienTai()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var dotDoAn = await _context.DotDoAns
                .Where(d => d.TrangThai == true
                    && d.NgayBatDauDkNguyenVong <= today
                    && d.NgayKetThucDkNguyenVong >= today)
                .FirstOrDefaultAsync();

            return dotDoAn;
        }

        // Tính GPA từ bảng KetQuaHocTap
        private async Task<double?> TinhGPA(int idSinhVien)
        {
            var ketQua = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == idSinhVien)
                .OrderByDescending(kq => kq.Id)
                .FirstOrDefaultAsync();

            return ketQua?.Gpa;  
        }

        #endregion
    }
}