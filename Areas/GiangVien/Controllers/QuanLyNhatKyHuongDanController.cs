using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLyNhatKyHuongDanController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyNhatKyHuongDanController(QuanLyDoAnTotNghiepContext context)
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
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                ViewBag.ThongBao = "Không tìm thấy thông tin giảng viên.";
                return View(new List<NhatKyHuongDanGVItem>().ToPagedList(1, pageSize));
            }

            ViewBag.TenGV = giangVien.IdNguoiDungNavigation?.HoTen;

            // Lấy đợt đồ án hiện tại
            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            if (dot == null)
            {
                ViewBag.ThongBao = "Hiện tại chưa có đợt đồ án nào đang hoạt động.";
                return View(new List<NhatKyHuongDanGVItem>().ToPagedList(1, pageSize));
            }

            // Lấy danh sách nhật ký theo đợt
            var data = await _context.NhatKyHuongDans
                .Where(n => n.IdDot == dot.Id)
                .OrderByDescending(n => n.NgayHop)
                .Select(n => new NhatKyHuongDanGVItem
                {
                    Id = n.Id,
                    TenGvhd = n.TenGvhd,
                    NgayHop = n.NgayHop.HasValue ? n.NgayHop.Value.ToString("dd/MM/yyyy") : "",
                    ThoiGian = n.ThoiGianHop.HasValue ? n.ThoiGianHop.Value.ToString("HH:mm") : "",
                    HinhThucHop = n.HinhThucHop,
                    DiaDiem = n.DiaDiemHop,
                    NoiDung = n.NoiDungHop,
                    MucTieu = n.MucTieuBuoiHop,
                    ThanhVien = n.ThanhVienThamDu,
                    ActionListJson = n.ActionList ?? "[]",
                    TaskCount = 0
                })
                .ToListAsync();

            return View(data.ToPagedList(pageNumber, pageSize));
        }
    }
}
