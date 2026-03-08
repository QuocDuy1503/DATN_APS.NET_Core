using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLyDangKyDeTaiController : Controller
    {
        private const int MAX_SV_PER_DETAI = 2;
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDangKyDeTaiController(QuanLyDoAnTotNghiepContext context)
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
        public async Task<IActionResult> Index(int? page, string? chuyenNganh, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentChuyenNganh = chuyenNganh;
            ViewBag.CurrentFilter = searchString;

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                ViewBag.ChuyenNganhs = new List<string>();
                return View(new List<DangKyDeTaiGVItem>().ToPagedList(1, pageSize));
            }

            var baseQuery = _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.SinhVienDeTais)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung);

            var allChuyenNganhs = await baseQuery
                .Where(dt => dt.IdChuyenNganhNavigation != null)
                .Select(dt => dt.IdChuyenNganhNavigation!.TenChuyenNganh)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
            ViewBag.ChuyenNganhs = allChuyenNganhs;

            var query = baseQuery.AsQueryable();

            if (!string.IsNullOrEmpty(chuyenNganh))
            {
                query = query.Where(dt => dt.IdChuyenNganhNavigation != null && dt.IdChuyenNganhNavigation.TenChuyenNganh == chuyenNganh);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dt => (dt.MaDeTai != null && dt.MaDeTai.Contains(searchString))
                    || (dt.TenDeTai != null && dt.TenDeTai.Contains(searchString)));
            }

            var data = query
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new DangKyDeTaiGVItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    ChuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    SoLuongDangKy = dt.SinhVienDeTais.Count
                });

            var pagedList = data.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null) return RedirectToAction("Index");

            var deTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdKhoaHocNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id && dt.IdGvhd == giangVien.IdNguoiDung);

            if (deTai == null) return NotFound();

            var allDangKy = deTai.SinhVienDeTais.ToList();
            var tongDaDuyet = allDangKy.Count(x => x.TrangThai == "DA_DUYET");

            var vm = new ChiTietDangKyDeTaiGVViewModel
            {
                IdDeTai = deTai.Id,
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                TenChuyenNganh = deTai.IdChuyenNganhNavigation?.TenChuyenNganh,
                NguoiDeXuat = deTai.IdNguoiDeXuatNavigation?.HoTen,
                MucTieu = deTai.MucTieuChinh,
                YeuCauTinhMoi = deTai.YeuCauTinhMoi,
                PhamVi = deTai.PhamViChucNang,
                CongNghe = deTai.CongNgheSuDung,
                KetQuaDuKien = deTai.SanPhamKetQuaDuKien,
                SoLuongDangKy = allDangKy.Count,
                SoLuongDaDuyet = tongDaDuyet,
                DanhSachSV = allDangKy
                    .OrderBy(svdt => svdt.TrangThai == "DA_DUYET" ? 0 : svdt.TrangThai == "Chờ GVHD duyệt" ? 1 : 2)
                    .Select(svdt => new SinhVienDangKyDetailGVItem
                    {
                        IdSvDeTai = svdt.Id,
                        Mssv = svdt.IdSinhVienNavigation?.Mssv,
                        HoTen = svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                        Email = svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.Email,
                        KhoaHoc = svdt.IdSinhVienNavigation?.IdKhoaHocNavigation?.TenKhoa,
                        TrangThai = svdt.TrangThai,
                        StatusCss = svdt.TrangThai switch
                        {
                            "DA_DUYET" => "member-approved",
                            "Chờ GVHD duyệt" => "member-pending",
                            "TU_CHOI" => "member-rejected",
                            _ => "member-default"
                        },
                        NgayDangKy = svdt.NgayDangKy?.ToString("dd/MM/yyyy HH:mm"),
                        NhanXet = svdt.NhanXet
                    }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetSinhVien([FromBody] DuyetSvRequest request)
        {
            var svDeTai = await _context.SinhVienDeTais.FindAsync(request.IdSvDeTai);
            if (svDeTai == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi đăng ký." });

            var deTai = await _context.DeTais.FindAsync(svDeTai.IdDeTai);
            if (deTai == null)
                return Json(new { success = false, message = "Không tìm thấy đề tài." });

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền duyệt sinh viên cho đề tài này." });

            if (request.Action == "approve")
            {
                var soSvDaDuyet = await _context.SinhVienDeTais
                    .CountAsync(s => s.IdDeTai == deTai.Id && s.TrangThai == "DA_DUYET");
                if (soSvDaDuyet >= MAX_SV_PER_DETAI)
                    return Json(new { success = false, message = $"Đề tài đã đủ số lượng sinh viên (tối đa {MAX_SV_PER_DETAI})." });

                svDeTai.TrangThai = "DA_DUYET";
            }
            else if (request.Action == "reject")
            {
                svDeTai.TrangThai = "TU_CHOI";
                svDeTai.NhanXet = request.NhanXet;
            }
            else
            {
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = request.Action == "approve" ? "Duyệt thành công!" : "Đã từ chối." });
        }
    }

    public class DuyetSvRequest
    {
        public int IdSvDeTai { get; set; }
        public string Action { get; set; } = "";
        public string? NhanXet { get; set; }
    }
}
