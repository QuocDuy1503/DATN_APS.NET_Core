using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    public class NopBaoCaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public NopBaoCaoController(QuanLyDoAnTotNghiepContext context)
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
                return View(new List<BaoCaoNop>());

            var idSinhVien = sinhVien.IdNguoiDung;

            var baoCaos = await _context.BaoCaoNops
                .Include(b => b.IdDeTaiNavigation)
                .Include(b => b.IdDotNavigation)
                .Where(b => b.IdSinhVien == idSinhVien)
                .OrderByDescending(b => b.NgayNop)
                .ToListAsync();

            return View(baoCaos);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var baoCao = await _context.BaoCaoNops
                .Include(b => b.IdDeTaiNavigation)
                .Include(b => b.IdDotNavigation)
                .Include(b => b.IdSinhVienNavigation)
                    .ThenInclude(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (baoCao == null)
                return NotFound();

            return View(baoCao);
        }
    }
}
