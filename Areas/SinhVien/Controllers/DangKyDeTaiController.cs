using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    [Area("SinhVien")]
    public class DangKyDeTaiController : Controller
    {
        private const int MAX_SV_PER_DETAI = 2;
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DangKyDeTaiController(QuanLyDoAnTotNghiepContext context)
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
        public async Task<IActionResult> Index(int? page, string? chuyenNganh, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentChuyenNganh = chuyenNganh;
            ViewBag.CurrentFilter = searchString;

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(x => x.Mssv == mssv);

            if (sinhVien == null)
            {
                ViewBag.Message = "Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.";
                ViewBag.GiaiDoan = "CHUA_MO";
                return View(new List<DangKyDeTaiViewModel>().ToPagedList(1, pageSize));
            }

            var dot = await GetDotDoAnActive();
            if (dot == null)
            {
                ViewBag.Message = "Hiện tại chưa có đợt đồ án nào đang hoạt động.";
                ViewBag.GiaiDoan = "CHUA_MO";
                return View(new List<DangKyDeTaiViewModel>().ToPagedList(1, pageSize));
            }

            var giaiDoan = XacDinhGiaiDoan(dot);
            ViewBag.GiaiDoan = giaiDoan;
            ViewBag.TenDot = dot.TenDot;

            if (giaiDoan == "CHUA_MO")
            {
                var ngayMo = dot.NgayBatDauDeXuatDeTai?.ToString("dd/MM/yyyy");
                ViewBag.Message = $"Giai đoạn đăng ký đề tài sẽ bắt đầu từ ngày {ngayMo}. Vui lòng quay lại sau.";
                return View(new List<DangKyDeTaiViewModel>().ToPagedList(1, pageSize));
            }

            var daDangKyIdDeTai = await _context.SinhVienDeTais
                .Where(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTaiNavigation!.IdDot == dot.Id)
                .Select(x => x.IdDeTai)
                .ToListAsync();

            var query = _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .Where(dt => dt.IdDot == dot.Id && dt.TrangThai == "DA_DUYET")
                .AsQueryable();

            // Build filter dropdown data from full (unfiltered) list
            var allChuyenNganhs = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Where(dt => dt.IdDot == dot.Id && dt.TrangThai == "DA_DUYET" && dt.IdChuyenNganhNavigation != null)
                .Select(dt => dt.IdChuyenNganhNavigation!.TenChuyenNganh)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
            ViewBag.ChuyenNganhs = allChuyenNganhs;

            // Apply filters
            if (!string.IsNullOrEmpty(chuyenNganh))
            {
                query = query.Where(dt => dt.IdChuyenNganhNavigation != null && dt.IdChuyenNganhNavigation.TenChuyenNganh == chuyenNganh);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dt => dt.MaDeTai!.Contains(searchString)
                    || dt.TenDeTai!.Contains(searchString)
                    || dt.IdGvhdNavigation!.IdNguoiDungNavigation!.HoTen!.Contains(searchString));
            }

            var data = query
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new DangKyDeTaiViewModel
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    Nganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    GVHD = dt.IdGvhdNavigation != null && dt.IdGvhdNavigation.IdNguoiDungNavigation != null
                        ? dt.IdGvhdNavigation.IdNguoiDungNavigation.HoTen : "",
                    TrangThai = dt.TrangThai,
                    StatusCss = dt.TrangThai == "DA_DUYET" ? "badge-green" :
                                dt.TrangThai == "CHO_DUYET" ? "badge-orange" :
                                dt.TrangThai == "TU_CHOI" ? "badge-red" : "badge-dark",
                    DaDangKy = daDangKyIdDeTai.Contains(dt.Id)
                });

            var pagedList = data.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dot = await GetDotDoAnActive();
            if (dot == null) return RedirectToAction("Index");

            var giaiDoan = XacDinhGiaiDoan(dot);
            if (giaiDoan == "CHUA_MO") return RedirectToAction("Index");

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(x => x.Mssv == mssv);
            if (sinhVien == null) return RedirectToAction("Index");

            var deTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id && dt.IdDot == dot.Id);
            if (deTai == null) return NotFound();

            var allDangKy = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(svdt => svdt.IdDeTai == id)
                .ToListAsync();

            var svId = sinhVien.IdNguoiDung;

            // Quy tắc quyền xem: SV chỉ thấy box của mình + box SV khác đã được duyệt
            var danhSachHienThi = allDangKy
                .Where(svdt => svdt.IdSinhVien == svId || svdt.TrangThai == "Đã duyệt")
                .OrderBy(svdt => svdt.TrangThai == "Đã duyệt" ? 0 : 1)
                .ThenBy(svdt => svdt.IdSinhVien == svId ? 1 : 0)
                .Select(svdt => new ThanhVienDangKyItem
                {
                    IdSinhVien = svdt.IdSinhVien ?? 0,
                    Mssv = svdt.IdSinhVienNavigation?.Mssv,
                    HoTen = svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    TrangThai = svdt.TrangThai,
                    StatusCss = svdt.TrangThai switch
                    {
                        "Đã duyệt" => "member-approved",
                        "Chờ GVHD duyệt" => "member-pending",
                        "Từ chối" => "member-rejected",
                        _ => "member-default"
                    },
                    NgayDangKy = svdt.NgayDangKy?.ToString("dd/MM/yyyy HH:mm"),
                    LaSvHienTai = svdt.IdSinhVien == svId
                })
                .ToList();

            var svDangKyTrongDot = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == svId
                    && x.IdDeTai != id
                    && x.IdDeTaiNavigation!.IdDot == dot.Id);

            var svDangKyDeTaiNay = allDangKy.Any(x => x.IdSinhVien == svId);
            var trangThaiSV = allDangKy.FirstOrDefault(x => x.IdSinhVien == svId)?.TrangThai;
            var tongDaDangKy = allDangKy.Count;
            var tongDaDuyet = allDangKy.Count(x => x.TrangThai == "Đã duyệt");

            var vm = new ChiTietDangKyDeTaiViewModel
            {
                Id = deTai.Id,
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                NguoiDeXuat = deTai.IdNguoiDeXuatNavigation?.HoTen,
                GVHD = deTai.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen,
                TenChuyenNganh = deTai.IdChuyenNganhNavigation?.TenChuyenNganh,
                MucTieu = deTai.MucTieuChinh,
                PhamVi = deTai.PhamViChucNang,
                CongNghe = deTai.CongNgheSuDung,
                YeuCauTinhMoi = deTai.YeuCauTinhMoi,
                KetQuaDuKien = deTai.SanPhamKetQuaDuKien,
                TrangThaiDeTai = deTai.TrangThai,

                MssvSinhVien = sinhVien.Mssv,
                HoTenSinhVien = sinhVien.IdNguoiDungNavigation?.HoTen,
                DaDangKyDeTaiNay = svDangKyDeTaiNay,
                DaDangKyDeTaiKhac = svDangKyTrongDot,
                TrangThaiDangKyCuaSV = trangThaiSV,
                GiaiDoan = giaiDoan,

                DanhSachThanhVien = danhSachHienThi,
                SoLuongDaDangKy = tongDaDangKy,
                SoLuongDaDuyet = tongDaDuyet,
                DaToanBoSlot = tongDaDangKy >= MAX_SV_PER_DETAI
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(int idDeTai)
        {
            var dot = await GetDotDoAnActive();
            if (dot == null)
                return Json(new { success = false, message = "Không tìm thấy đợt đồ án." });

            var giaiDoan = XacDinhGiaiDoan(dot);
            if (giaiDoan != "DANG_MO")
                return Json(new { success = false, message = "Giai đoạn đăng ký đã đóng." });

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(x => x.Mssv == mssv);
            if (sinhVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });

            var deTai = await _context.DeTais.FirstOrDefaultAsync(dt => dt.Id == idDeTai && dt.IdDot == dot.Id);
            if (deTai == null)
                return Json(new { success = false, message = "Đề tài không tồn tại hoặc không thuộc đợt hiện tại." });

            var daDangKy = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTai == idDeTai);
            if (daDangKy)
                return Json(new { success = false, message = "Bạn đã đăng ký đề tài này rồi." });

            var daDangKyDeTaiKhac = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTaiNavigation!.IdDot == dot.Id);
            if (daDangKyDeTaiKhac)
                return Json(new { success = false, message = "Bạn đã đăng ký 1 đề tài khác trong đợt này." });

            var slotCount = await _context.SinhVienDeTais.CountAsync(x => x.IdDeTai == idDeTai);
            if (slotCount >= MAX_SV_PER_DETAI)
                return Json(new { success = false, message = "Đề tài đã đủ số lượng sinh viên đăng ký." });

            try
            {
                var svdt = new SinhVienDeTai
                {
                    IdDeTai = deTai.Id,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    TrangThai = "Chờ GVHD duyệt",
                    NgayDangKy = DateTime.Now
                };
                _context.SinhVienDeTais.Add(svdt);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đăng ký đề tài thành công! Vui lòng chờ giảng viên duyệt." });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDangKy(int idDeTai)
        {
            var dot = await GetDotDoAnActive();
            if (dot == null)
                return Json(new { success = false, message = "Không tìm thấy đợt đồ án." });

            var giaiDoan = XacDinhGiaiDoan(dot);
            if (giaiDoan != "DANG_MO")
                return Json(new { success = false, message = "Giai đoạn đăng ký đã đóng, không thể hủy." });

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(x => x.Mssv == mssv);
            if (sinhVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });

            var svdt = await _context.SinhVienDeTais
                .FirstOrDefaultAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTai == idDeTai);
            if (svdt == null)
                return Json(new { success = false, message = "Bạn chưa đăng ký đề tài này." });

            if (svdt.TrangThai != "Chờ GVHD duyệt")
                return Json(new { success = false, message = "Chỉ có thể hủy khi đang ở trạng thái 'Chờ GVHD duyệt'." });

            try
            {
                _context.SinhVienDeTais.Remove(svdt);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã hủy đăng ký đề tài thành công." });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }

        // ================= PRIVATE =================

        private async Task<DotDoAn?> GetDotDoAnActive()
        {
            return await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
        }

        private static string XacDinhGiaiDoan(DotDoAn dot)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (dot.NgayBatDauDeXuatDeTai == null || dot.NgayKetThucDeXuatDeTai == null)
                return "CHUA_MO";

            if (today < dot.NgayBatDauDeXuatDeTai)
                return "CHUA_MO";

            if (today > dot.NgayKetThucDeXuatDeTai)
                return "DA_KET_THUC";

            return "DANG_MO";
        }

        private static string MapTrangThaiCss(string? trangThai)
        {
            return trangThai switch
            {
                "DA_DUYET" => "badge-green",
                "CHO_DUYET" => "badge-orange",
                "TU_CHOI" => "badge-red",
                _ => "badge-dark"
            };
        }
    }
}
