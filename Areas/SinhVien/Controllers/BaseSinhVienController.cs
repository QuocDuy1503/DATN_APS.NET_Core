using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Base controller cho tất cả các controller trong Area SinhVien
    /// Kiểm tra đăng nhập và trạng thái duyệt nguyện vọng
    /// </summary>
    [Area("SinhVien")]
    public abstract class BaseSinhVienController : Controller
    {
        protected readonly QuanLyDoAnTotNghiepContext _context;

        protected BaseSinhVienController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Danh sách các action không cần kiểm tra nguyện vọng
        /// </summary>
        protected virtual List<string> ActionsKhongCanKiemTraNguyenVong => new() { "Index", "TrangChu" };

        /// <summary>
        /// Có bỏ qua kiểm tra nguyện vọng cho controller này không
        /// Override trong các controller con nếu cần bỏ qua
        /// </summary>
        protected virtual bool BoQuaKiemTraNguyenVong => false;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // 1. Kiểm tra đăng nhập
            var sessionRole = HttpContext.Session.GetString("Role");
            var isStudentByClaim = User?.Identity?.IsAuthenticated == true && (User.IsInRole("SINH_VIEN") || User.IsInRole("SV"));
            var isStudentBySession = sessionRole == "SINH_VIEN" || sessionRole == "SV";

            if (!isStudentByClaim && !isStudentBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }

            // 2. Kiểm tra nguyện vọng đã được duyệt chưa (trừ các action được loại trừ)
            if (!BoQuaKiemTraNguyenVong)
            {
                var actionName = context.ActionDescriptor.RouteValues["action"] ?? "";
                
                // Không kiểm tra cho các action trong danh sách loại trừ
                if (!ActionsKhongCanKiemTraNguyenVong.Contains(actionName))
                {
                    var ketQuaKiemTra = KiemTraNguyenVongDaDuyet().GetAwaiter().GetResult();
                    
                    if (!ketQuaKiemTra.DaDuyet)
                    {
                        // Chuyển hướng đến trang thông báo
                        TempData["ErrorMessage"] = ketQuaKiemTra.ThongBao;
                        TempData["ErrorType"] = ketQuaKiemTra.LoaiLoi;
                        context.Result = RedirectToAction("ChuaDuyetNguyenVong", "ThongBaoLoi", new { area = "SinhVien" });
                        return;
                    }
                }
            }

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Kiểm tra sinh viên đã được duyệt nguyện vọng trong đợt hiện tại chưa
        /// </summary>
        protected async Task<KetQuaKiemTraNguyenVong> KiemTraNguyenVongDaDuyet()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            
            if (string.IsNullOrEmpty(mssv))
            {
                return new KetQuaKiemTraNguyenVong
                {
                    DaDuyet = false,
                    ThongBao = "Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.",
                    LoaiLoi = "KHONG_TIM_THAY_SV"
                };
            }

            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(sv => sv.Mssv == mssv);
            if (sinhVien == null)
            {
                return new KetQuaKiemTraNguyenVong
                {
                    DaDuyet = false,
                    ThongBao = "Không tìm thấy thông tin sinh viên trong hệ thống.",
                    LoaiLoi = "KHONG_TIM_THAY_SV"
                };
            }

            // Lấy đợt đồ án đang hoạt động
            var dotHienTai = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (dotHienTai == null)
            {
                return new KetQuaKiemTraNguyenVong
                {
                    DaDuyet = false,
                    ThongBao = "Hiện tại chưa có đợt đồ án nào đang hoạt động.",
                    LoaiLoi = "KHONG_CO_DOT"
                };
            }

            // Kiểm tra đăng ký nguyện vọng
            var dangKyNguyenVong = await _context.DangKyNguyenVongs
                .FirstOrDefaultAsync(dk => dk.IdSinhVien == sinhVien.IdNguoiDung && dk.IdDot == dotHienTai.Id);

            if (dangKyNguyenVong == null)
            {
                return new KetQuaKiemTraNguyenVong
                {
                    DaDuyet = false,
                    ThongBao = "Bạn chưa đăng ký nguyện vọng làm đồ án trong đợt này. Vui lòng đăng ký nguyện vọng trước.",
                    LoaiLoi = "CHUA_DANG_KY",
                    TenDot = dotHienTai.TenDot
                };
            }

            // TrangThai: 0 = Chờ duyệt, 1 = Đạt, 2 = Không đạt
            if (dangKyNguyenVong.TrangThai == 0)
            {
                return new KetQuaKiemTraNguyenVong
                {
                    DaDuyet = false,
                    ThongBao = "Nguyện vọng của bạn đang chờ được duyệt. Vui lòng chờ kết quả từ phòng Đào tạo.",
                    LoaiLoi = "CHO_DUYET",
                    TenDot = dotHienTai.TenDot
                };
            }

            if (dangKyNguyenVong.TrangThai == 2)
            {
                return new KetQuaKiemTraNguyenVong
                {
                    DaDuyet = false,
                    ThongBao = "Nguyện vọng của bạn không được duyệt do chưa đủ điều kiện. Vui lòng liên hệ phòng Đào tạo để biết thêm chi tiết.",
                    LoaiLoi = "KHONG_DAT",
                    TenDot = dotHienTai.TenDot
                };
            }

            // TrangThai = 1 => Đạt
            return new KetQuaKiemTraNguyenVong
            {
                DaDuyet = true,
                ThongBao = "Nguyện vọng đã được duyệt.",
                LoaiLoi = null,
                TenDot = dotHienTai.TenDot
            };
        }

        /// <summary>
        /// Lấy đợt đồ án đang hoạt động
        /// </summary>
        protected async Task<DotDoAn?> GetDotDoAnActive()
        {
            return await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lấy thông tin sinh viên hiện tại
        /// </summary>
        protected async Task<DATN_TMS.Models.SinhVien?> GetSinhVienHienTai()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            if (string.IsNullOrEmpty(mssv)) return null;

            return await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);
        }
    }

    /// <summary>
    /// Kết quả kiểm tra nguyện vọng
    /// </summary>
    public class KetQuaKiemTraNguyenVong
    {
        public bool DaDuyet { get; set; }
        public string? ThongBao { get; set; }
        public string? LoaiLoi { get; set; }
        public string? TenDot { get; set; }
    }
}
