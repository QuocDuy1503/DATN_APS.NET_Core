using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class BangKeHoachSinhVienController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public BangKeHoachSinhVienController(QuanLyDoAnTotNghiepContext context)
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

        private async Task<List<int?>> GetSinhVienIdsOfGV()
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null) return new List<int?>();

            return await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .Where(svdt => svdt.IdDeTaiNavigation != null && svdt.IdDeTaiNavigation.IdGvhd == giangVien.IdNguoiDung)
                .Select(svdt => svdt.IdSinhVien)
                .Distinct()
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var idSinhViens = await GetSinhVienIdsOfGV();

            if (!idSinhViens.Any())
                return View(new List<KeHoachSVGVItem>());

            var data = await _context.KeHoachCongViecs
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(k => idSinhViens.Contains(k.IdSinhVien))
                .OrderBy(k => k.Stt).ThenBy(k => k.Id)
                .Select(k => new KeHoachSVGVItem
                {
                    Id = k.Id,
                    Stt = k.Stt,
                    TenCongViec = k.TenCongViec,
                    NguoiThucHien = k.NguoiPhuTrach ?? (k.IdSinhVienNavigation != null && k.IdSinhVienNavigation.IdNguoiDungNavigation != null
                        ? k.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : ""),
                    NgayBatDau = k.NgayBatDau.HasValue ? k.NgayBatDau.Value.ToString("dd/MM/yyyy") : "",
                    NgayKetThuc = k.NgayKetThuc.HasValue ? k.NgayKetThuc.Value.ToString("dd/MM/yyyy") : "",
                    NgayBDThucTe = k.NgayBatDauThucTe.HasValue ? k.NgayBatDauThucTe.Value.ToString("dd/MM/yyyy") : "",
                    NgayKTThucTe = k.NgayKetThucThucTe.HasValue ? k.NgayKetThucThucTe.Value.ToString("dd/MM/yyyy") : "",
                    TrangThai = k.TrangThai,
                    StatusCss = k.TrangThai == "Đã duyệt" ? "status-completed"
                              : k.TrangThai == "Đang thực hiện" ? "status-running"
                              : k.TrangThai == "Chờ GV duyệt" ? "status-waiting"
                              : "status-pending",
                    StatusText = k.TrangThai ?? "Chưa thực hiện"
                })
                .ToListAsync();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetKeHoach([FromBody] DuyetKeHoachRequest request)
        {
            var keHoach = await _context.KeHoachCongViecs.FindAsync(request.Id);
            if (keHoach == null)
                return Json(new { success = false, message = "Không tìm thấy kế hoạch." });

            var idSinhViens = await GetSinhVienIdsOfGV();
            if (!idSinhViens.Contains(keHoach.IdSinhVien))
                return Json(new { success = false, message = "Bạn không có quyền duyệt kế hoạch này." });

            if (keHoach.TrangThai != "Chờ GV duyệt")
                return Json(new { success = false, message = "Kế hoạch không ở trạng thái chờ duyệt." });

            keHoach.TrangThai = request.Action == "approve" ? "Đã duyệt" : "Đang thực hiện";
            keHoach.GhiChu = request.NhanXet;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = request.Action == "approve" ? "Duyệt thành công!" : "Đã yêu cầu chỉnh sửa." });
        }
    }

    public class DuyetKeHoachRequest
    {
        public int Id { get; set; }
        public string Action { get; set; } = "";
        public string? NhanXet { get; set; }
    }
}
