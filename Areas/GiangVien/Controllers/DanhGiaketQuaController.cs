using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class DanhGiaketQuaController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DanhGiaketQuaController(QuanLyDoAnTotNghiepContext context)
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
                return View(new List<DanhGiaKetQuaGVItem>());

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
                .Select(dt => new DanhGiaKetQuaGVItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    ChuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    SoLuongSV = dt.SinhVienDeTais.Count(svdt => svdt.TrangThai == "DA_DUYET"),
                    DanhSachSV = dt.SinhVienDeTais
                        .Where(svdt => svdt.TrangThai == "DA_DUYET")
                        .Select(svdt => new SinhVienDGItem
                        {
                            Mssv = svdt.IdSinhVienNavigation != null ? svdt.IdSinhVienNavigation.Mssv : "",
                            HoTen = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdNguoiDungNavigation != null
                                ? svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen : "",
                            KhoaHoc = svdt.IdSinhVienNavigation != null && svdt.IdSinhVienNavigation.IdKhoaHocNavigation != null
                                ? svdt.IdSinhVienNavigation.IdKhoaHocNavigation.TenKhoa : ""
                        }).ToList()
                })
                .ToListAsync();

            return View(data);
        }
    }
}
