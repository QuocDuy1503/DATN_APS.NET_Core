using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLySinhVienController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLySinhVienController(QuanLyDoAnTotNghiepContext context)
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
                return View(new List<SinhVienGVItem>());

            var idDeTais = await _context.DeTais
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung)
                .Select(dt => dt.Id)
                .ToListAsync();

            var data = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdChuyenNganhNavigation)
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdKhoaHocNavigation)
                .Include(svdt => svdt.IdDeTaiNavigation)
                .Where(svdt => svdt.IdDeTai.HasValue && idDeTais.Contains(svdt.IdDeTai.Value))
                .Select(svdt => new SinhVienGVItem
                {
                    Mssv = svdt.IdSinhVienNavigation != null ? svdt.IdSinhVienNavigation.Mssv : "",
                    HoTen = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                        ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : "",
                    Email = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                        ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.Email : "",
                    ChuyenNganh = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdChuyenNganhNavigation != null
                        ? svdt.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    KhoaHoc = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdKhoaHocNavigation != null
                        ? svdt.IdSinhVienNavigation.IdKhoaHocNavigation.TenKhoa : "",
                    TenDeTai = svdt.IdDeTaiNavigation != null ? svdt.IdDeTaiNavigation.TenDeTai : "",
                    TrangThai = svdt.TrangThai
                })
                .ToListAsync();

            return View(data);
        }
    }
}
