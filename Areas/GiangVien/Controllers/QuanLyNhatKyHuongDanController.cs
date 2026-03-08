using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Index()
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
                return View(new List<NhatKyHuongDanGVItem>());

            ViewBag.TenGV = giangVien.IdNguoiDungNavigation?.HoTen;

            var data = await _context.NhatKyHuongDans
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

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] NhatKyGVCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NgayHop) || string.IsNullOrWhiteSpace(dto.ThoiGianHop)
                || string.IsNullOrWhiteSpace(dto.HinhThucHop) || string.IsNullOrWhiteSpace(dto.MucTieuBuoiHop)
                || string.IsNullOrWhiteSpace(dto.NoiDungHop))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ các trường bắt buộc." });
            }

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();

            var nhatKy = new NhatKyHuongDan
            {
                IdDot = dot?.Id,
                NgayHop = DateOnly.TryParse(dto.NgayHop, out var nh) ? nh : null,
                ThoiGianHop = TimeOnly.TryParse(dto.ThoiGianHop, out var th) ? th : null,
                HinhThucHop = dto.HinhThucHop,
                DiaDiemHop = dto.DiaDiemHop,
                ThanhVienThamDu = dto.ThanhVienThamDu,
                TenGvhd = dto.TenGvhd ?? giangVien?.IdNguoiDungNavigation?.HoTen,
                MucTieuBuoiHop = dto.MucTieuBuoiHop,
                NoiDungHop = dto.NoiDungHop,
                ActionList = dto.ActionList
            };

            _context.NhatKyHuongDans.Add(nhatKy);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class NhatKyGVCreateDto
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
