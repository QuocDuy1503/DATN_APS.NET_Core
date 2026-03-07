using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    public class QuanLyKeHoachController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyKeHoachController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isStudentByClaim = User?.Identity?.IsAuthenticated == true && (User.IsInRole("SINH_VIEN") || User.IsInRole("SV"));
            var isStudentBySession = sessionRole == "SINH_VIEN" || sessionRole == "SV";

            if (!isStudentByClaim && !isStudentBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }
            base.OnActionExecuting(context);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
                return View(new List<KeHoachCongViec>());

            var idSinhVien = sinhVien.IdNguoiDung;

            var keHoachs = await _context.KeHoachCongViecs
                .Include(k => k.IdDotNavigation)
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv.IdNguoiDungNavigation)
                .Where(k => k.IdSinhVien == idSinhVien)
                .OrderBy(k => k.Stt)
                .ToListAsync();

            return View(keHoachs);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var keHoach = await _context.KeHoachCongViecs
                .Include(k => k.IdDotNavigation)
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv.IdNguoiDungNavigation)
                .Include(k => k.IdFileMinhChungNavigation)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (keHoach == null)
                return NotFound();

            return View(keHoach);
        }
    }
}
