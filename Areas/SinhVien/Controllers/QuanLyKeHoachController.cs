using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller quản lý kế hoạch công việc cho sinh viên
    /// Kế thừa BaseSinhVienController để kiểm tra nguyện vọng đã duyệt
    /// </summary>
    public class QuanLyKeHoachController : BaseSinhVienController
    {
        private readonly IWebHostEnvironment _env;

        public QuanLyKeHoachController(QuanLyDoAnTotNghiepContext context, IWebHostEnvironment env) : base(context)
        {
            _env = env;
        }

        // ============ INDEX ============
        [HttpGet]
        public async Task<IActionResult> Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var vm = new KeHoachIndexViewModel();

            var dot = await GetDotDoAnActive();
            if (dot == null)
            {
                vm.ThongBao = "Hiện tại chưa có đợt đồ án nào đang hoạt động.";
                return View(vm);
            }

            vm.TenDot = dot.TenDot;

            var sinhVien = await GetSinhVienDangNhap();
            if (sinhVien == null)
            {
                vm.ThongBao = "Không tìm thấy thông tin sinh viên. Vui lòng đăng nhập lại.";
                return View(vm);
            }

            // Kiểm tra SV đã có đề tài được GVHD duyệt
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                    .ThenInclude(dt => dt!.IdGvhdNavigation)
                        .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            if (svDeTai == null)
            {
                vm.ThongBao = "Bạn chưa được duyệt vào đề tài nào. Vui lòng chờ giảng viên duyệt đăng ký đề tài.";
                return View(vm);
            }

            // ============================================
            // BUSINESS RULE: Đề tài phải được HỘI ĐỒNG DUYỆT (TrangThai = DA_DUYET)
            // ============================================
            var deTai = svDeTai.IdDeTaiNavigation;
            if (deTai == null || deTai.TrangThai != "DA_DUYET")
            {
                vm.ThongBao = "Đề tài của bạn chưa được hội đồng duyệt. Vui lòng chờ hội đồng xét duyệt đề tài.";
                vm.MaDeTai = deTai?.MaDeTai;
                vm.TenDeTai = deTai?.TenDeTai;
                vm.TrangThaiDeTai = deTai?.TrangThai;
                return View(vm);
            }

            // Đủ điều kiện — load dữ liệu
            vm.GiaiDoan = "DA_MO";
            vm.MaDeTai = deTai.MaDeTai;
            vm.TenDeTai = deTai.TenDeTai;
            vm.TenGVHD = deTai.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen;

            var keHoachs = await _context.KeHoachCongViecs
                .Where(k => k.IdSinhVien == sinhVien.IdNguoiDung && k.IdDot == dot.Id)
                .OrderBy(k => k.Stt).ThenBy(k => k.Id)
                .ToListAsync();

            vm.DanhSachCongViec = keHoachs.Select(k => new KeHoachItemViewModel
            {
                Id = k.Id,
                Stt = k.Stt,
                TenCongViec = k.TenCongViec,
                NguoiPhuTrach = k.NguoiPhuTrach,
                ThoiGianDuKien = FormatDateRange(k.NgayBatDau, k.NgayKetThuc),
                ThoiGianThucTe = FormatDateRange(k.NgayBatDauThucTe, k.NgayKetThucThucTe),
                TrangThai = k.TrangThai,
                StatusCss = MapStatusCss(k.TrangThai),
                StatusText = MapStatusText(k.TrangThai)
            }).ToList().ToPagedList(pageNumber, pageSize);

            return View(vm);
        }

        // ============ CREATE GET ============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var dsSinhVien = await GetDanhSachSinhVienCungDeTai();
            var vm = new KeHoachCreateViewModel { DanhSachSinhVienDeTai = dsSinhVien };
            return PartialView("_CreateModal", vm);
        }

        // ============ CREATE POST ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string tenCongViec, string moTaCongViec, string nguoiPhuTrach, string ngayBatDau, string ngayKetThuc)
        {
            if (string.IsNullOrWhiteSpace(tenCongViec) || string.IsNullOrWhiteSpace(moTaCongViec) || string.IsNullOrWhiteSpace(nguoiPhuTrach))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin bắt buộc." });

            if (!DateOnly.TryParse(ngayBatDau, out var bd) || !DateOnly.TryParse(ngayKetThuc, out var kt))
                return Json(new { success = false, message = "Ngày không hợp lệ." });

            if (kt <= bd)
                return Json(new { success = false, message = "Ngày kết thúc phải sau ngày bắt đầu." });

            var dot = await GetDotDoAnActive();
            if (dot == null) return Json(new { success = false, message = "Không tìm thấy đợt đồ án." });

            var sinhVien = await GetSinhVienDangNhap();
            if (sinhVien == null) return Json(new { success = false, message = "Không tìm thấy sinh viên." });

            var maxStt = await _context.KeHoachCongViecs
                .Where(k => k.IdSinhVien == sinhVien.IdNguoiDung && k.IdDot == dot.Id)
                .MaxAsync(k => (int?)k.Stt) ?? 0;

            var keHoach = new KeHoachCongViec
            {
                Stt = maxStt + 1,
                IdSinhVien = sinhVien.IdNguoiDung,
                IdDot = dot.Id,
                TenCongViec = tenCongViec.Trim(),
                MoTaCongViec = moTaCongViec.Trim(),
                NguoiPhuTrach = nguoiPhuTrach.Trim(),
                NgayBatDau = bd,
                NgayKetThuc = kt,
                TrangThai = "Chưa thực hiện"
            };

            _context.KeHoachCongViecs.Add(keHoach);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm kế hoạch thành công!" });
        }

        // ============ DETAIL GET ============
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var sinhVien = await GetSinhVienDangNhap();
            if (sinhVien == null) return RedirectToAction("Index");

            var keHoach = await _context.KeHoachCongViecs
                .Include(k => k.IdDotNavigation)
                .Include(k => k.IdFileMinhChungNavigation)
                .FirstOrDefaultAsync(k => k.Id == id && k.IdSinhVien == sinhVien.IdNguoiDung);

            if (keHoach == null) return NotFound();

            var dsSinhVien = await GetDanhSachSinhVienCungDeTai();
            var nhanXet = keHoach.IdFileMinhChungNavigation?.NhanXet;

            var vm = new KeHoachDetailViewModel
            {
                Id = keHoach.Id,
                Stt = keHoach.Stt,
                TenCongViec = keHoach.TenCongViec,
                MoTaCongViec = keHoach.MoTaCongViec,
                NguoiPhuTrach = keHoach.NguoiPhuTrach,
                NgayBatDau = keHoach.NgayBatDau?.ToString("yyyy-MM-dd"),
                NgayKetThuc = keHoach.NgayKetThuc?.ToString("yyyy-MM-dd"),
                NgayBatDauThucTe = keHoach.NgayBatDauThucTe?.ToString("yyyy-MM-dd"),
                NgayKetThucThucTe = keHoach.NgayKetThucThucTe?.ToString("yyyy-MM-dd"),
                TrangThai = keHoach.TrangThai,
                StatusText = MapStatusText(keHoach.TrangThai),
                StatusCss = MapStatusCss(keHoach.TrangThai),
                GhiChu = keHoach.GhiChu,
                TenDot = keHoach.IdDotNavigation?.TenDot,
                IdFileMinhChung = keHoach.IdFileMinhChung,
                TenFileMinhChung = keHoach.IdFileMinhChungNavigation?.TenBaoCao,
                LinkFileMinhChung = keHoach.IdFileMinhChungNavigation?.FileBaocao,
                NhanXetGiangVien = nhanXet,
                DanhSachSinhVienDeTai = dsSinhVien,
                IsEditable = keHoach.TrangThai != "Đã duyệt"
            };

            return View(vm);
        }

        // ============ UPDATE POST ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, string tenCongViec, string moTaCongViec, string nguoiPhuTrach,
            string ngayBatDau, string ngayKetThuc, string? ngayBatDauThucTe, string? ngayKetThucThucTe,
            string trangThai, string? ghiChu)
        {
            var sinhVien = await GetSinhVienDangNhap();
            if (sinhVien == null) return Json(new { success = false, message = "Không tìm thấy sinh viên." });

            var keHoach = await _context.KeHoachCongViecs
                .FirstOrDefaultAsync(k => k.Id == id && k.IdSinhVien == sinhVien.IdNguoiDung);
            if (keHoach == null) return Json(new { success = false, message = "Không tìm thấy công việc." });

            if (keHoach.TrangThai == "Đã duyệt")
                return Json(new { success = false, message = "Công việc đã được duyệt, không thể chỉnh sửa." });

            // SV không được tự set "Đã duyệt"
            var allowedStatuses = new[] { "Chưa thực hiện", "Đang thực hiện", "Chờ GV duyệt" };
            if (!allowedStatuses.Contains(trangThai))
                return Json(new { success = false, message = "Trạng thái không hợp lệ." });

            keHoach.TenCongViec = tenCongViec?.Trim();
            keHoach.MoTaCongViec = moTaCongViec?.Trim();
            keHoach.NguoiPhuTrach = nguoiPhuTrach?.Trim();
            keHoach.TrangThai = trangThai;
            keHoach.GhiChu = ghiChu?.Trim();

            if (DateOnly.TryParse(ngayBatDau, out var bd)) keHoach.NgayBatDau = bd;
            if (DateOnly.TryParse(ngayKetThuc, out var kt)) keHoach.NgayKetThuc = kt;
            keHoach.NgayBatDauThucTe = DateOnly.TryParse(ngayBatDauThucTe, out var bdtt) ? bdtt : null;
            keHoach.NgayKetThucThucTe = DateOnly.TryParse(ngayKetThucThucTe, out var kttt) ? kttt : null;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // ============ UPLOAD FILE POST ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadFile(int id, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file." });

            var sinhVien = await GetSinhVienDangNhap();
            if (sinhVien == null) return Json(new { success = false, message = "Không tìm thấy sinh viên." });

            var keHoach = await _context.KeHoachCongViecs
                .FirstOrDefaultAsync(k => k.Id == id && k.IdSinhVien == sinhVien.IdNguoiDung);
            if (keHoach == null) return Json(new { success = false, message = "Không tìm thấy công việc." });

            if (keHoach.TrangThai == "Đã duyệt")
                return Json(new { success = false, message = "Công việc đã duyệt, không thể nộp file." });

            var dot = await GetDotDoAnActive();
            var svDeTai = await _context.SinhVienDeTais
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation!.IdDot == dot!.Id && svdt.TrangThai == "DA_DUYET");

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "kehoach");
            if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

            var mssv = HttpContext.Session.GetString("UserCode");
            var uniqueName = $"{mssv}_KH{id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/kehoach/{uniqueName}";

            var baoCao = new BaoCaoNop
            {
                IdDot = dot?.Id,
                IdDeTai = svDeTai?.IdDeTai,
                IdSinhVien = sinhVien.IdNguoiDung,
                TenBaoCao = file.FileName,
                FileBaocao = relativePath,
                NgayNop = DateTime.Now,
                TrangThai = "Đã nộp",
                LoaiBaoCao = "MINH_CHUNG",
                NgaySuaDoiCuoi = DateTime.Now
            };
            _context.BaoCaoNops.Add(baoCao);
            await _context.SaveChangesAsync();

            keHoach.IdFileMinhChung = baoCao.Id;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Nộp file minh chứng thành công!" });
        }

        // ============ API TÌM SINH VIÊN ============
        [HttpGet]
        public async Task<IActionResult> TimSinhVien(string? keyword)
        {
            var dsSinhVien = await GetDanhSachSinhVienCungDeTai();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.ToLower();
                dsSinhVien = dsSinhVien
                    .Where(s => (s.Mssv ?? "").ToLower().Contains(kw) || (s.HoTen ?? "").ToLower().Contains(kw))
                    .ToList();
            }
            return Json(dsSinhVien);
        }

        // ============ PRIVATE METHODS ============
        private async Task<DotDoAn?> GetDotDoAnActive()
        {
            return await _context.DotDoAns
                .Where(d => d.TrangThai == true)
                .OrderByDescending(d => d.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<DATN_TMS.Models.SinhVien?> GetSinhVienDangNhap()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            return await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);
        }

        private async Task<List<SinhVienGợiYItem>> GetDanhSachSinhVienCungDeTai()
        {
            var sinhVien = await GetSinhVienDangNhap();
            if (sinhVien == null) return new();

            var dot = await GetDotDoAnActive();
            if (dot == null) return new();

            var svDeTai = await _context.SinhVienDeTais
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation!.IdDot == dot.Id && svdt.TrangThai == "DA_DUYET");
            if (svDeTai == null) return new();

            return await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation).ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(svdt => svdt.IdDeTai == svDeTai.IdDeTai && svdt.TrangThai == "DA_DUYET")
                .Select(svdt => new SinhVienGợiYItem
                {
                    IdSinhVien = svdt.IdSinhVien ?? 0,
                    Mssv = svdt.IdSinhVienNavigation!.Mssv,
                    HoTen = svdt.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen
                })
                .ToListAsync();
        }

        private static string FormatDateRange(DateOnly? from, DateOnly? to)
        {
            if (from == null && to == null) return "";
            var f = from?.ToString("dd/MM/yyyy") ?? "---";
            var t = to?.ToString("dd/MM/yyyy") ?? "---";
            return $"{f} - {t}";
        }

        private static string MapStatusCss(string? status) => status switch
        {
            "Chưa thực hiện" => "status-pending",
            "Đang thực hiện" => "status-running",
            "Chờ GV duyệt" => "status-waiting",
            "Đã duyệt" => "status-completed",
            _ => "status-pending"
        };

        private static string MapStatusText(string? status) => status switch
        {
            "Chưa thực hiện" => "Chưa thực hiện",
            "Đang thực hiện" => "Đang thực hiện",
            "Chờ GV duyệt" => "Chờ GV duyệt",
            "Đã duyệt" => "Đã duyệt",
            _ => status ?? "---"
        };
    }
}
