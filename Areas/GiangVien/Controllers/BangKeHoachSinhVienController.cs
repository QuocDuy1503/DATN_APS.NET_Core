using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

// Alias để tránh xung đột namespace
using GiangVienModel = DATN_TMS.Models.GiangVien;

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

        /// <summary>
        /// Lấy thông tin giảng viên hiện tại
        /// </summary>
        private async Task<GiangVienModel?> GetCurrentGiangVien()
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            return await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);
        }

        /// <summary>
        /// Lấy đợt đồ án hiện tại
        /// </summary>
        private async Task<DotDoAn?> GetDotHienTai()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .Where(d => d.TrangThai == true &&
                           (!d.NgayKetThucDot.HasValue || d.NgayKetThucDot.Value >= today))
                .OrderByDescending(d => d.NgayBatDauDot)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Hiển thị danh sách đề tài mà GV đang hướng dẫn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var giangVien = await GetCurrentGiangVien();
            if (giangVien == null)
                return View(new BangKeHoachSVIndexViewModel { CoDot = false });

            var dotHienTai = await GetDotHienTai();
            if (dotHienTai == null)
                return View(new BangKeHoachSVIndexViewModel { CoDot = false });

            // Lấy danh sách đề tài mà GV đang hướng dẫn trong đợt này
            var danhSachDeTai = await _context.DeTais
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung &&
                             dt.IdDot == dotHienTai.Id)
                .ToListAsync();

            var deTaiItems = new List<DeTaiKeHoachItem>();

            foreach (var dt in danhSachDeTai)
            {
                // Lấy danh sách sinh viên đã duyệt của đề tài
                var sinhVienDaDuyet = dt.SinhVienDeTais
                    .Where(svdt => svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt")
                    .ToList();

                if (!sinhVienDaDuyet.Any()) continue;

                var idSinhViens = sinhVienDaDuyet.Select(sv => sv.IdSinhVien).ToList();

                // Lấy kế hoạch công việc của các sinh viên TRONG ĐỢT HIỆN TẠI
                var keHoachs = await _context.KeHoachCongViecs
                    .Where(k => idSinhViens.Contains(k.IdSinhVien) && k.IdDot == dotHienTai.Id)
                    .ToListAsync();

                // Tính tiến độ theo trạng thái mới
                var tongCongViec = keHoachs.Count;
                double tienDo = 0;
                if (tongCongViec > 0)
                {
                    var diemMoiTask = 100.0 / tongCongViec;
                    foreach (var kh in keHoachs)
                    {
                        // Đã duyệt = 100%
                        if (kh.TrangThai == "Đã duyệt" || kh.TrangThai == "DA_DUYET")
                            tienDo += diemMoiTask;
                        // Chờ duyệt (SV đã hoàn thành, chờ GV duyệt) = 75%
                        else if (kh.TrangThai == "Chờ GV duyệt" || kh.TrangThai == "CHO_DUYET" || 
                                 kh.TrangThai == "Hoàn thành")
                            tienDo += diemMoiTask * 0.75;
                        // Đang thực hiện = 50%
                        else if (kh.TrangThai == "Đang thực hiện" || kh.TrangThai == "DANG_THUC_HIEN")
                            tienDo += diemMoiTask * 0.5;
                    }
                }

                var item = new DeTaiKeHoachItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    SoLuongThanhVien = sinhVienDaDuyet.Count,
                    SoLuongCongViec = tongCongViec,
                    SoLuongChoXacNhan = keHoachs.Count(k => k.TrangThai == "Chờ GV duyệt" || 
                                                            k.TrangThai == "CHO_DUYET" || 
                                                            k.TrangThai == "Hoàn thành"),
                    SoLuongDaHoanThanh = keHoachs.Count(k => k.TrangThai == "Đã duyệt" || k.TrangThai == "DA_DUYET"),
                    TienDo = Math.Round(tienDo, 1),
                    TienDoCss = tienDo >= 80 ? "progress-good" : tienDo >= 50 ? "progress-medium" : "progress-low",
                    DanhSachSinhVien = sinhVienDaDuyet
                        .Select(sv => sv.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen ?? "")
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList()
                };

                deTaiItems.Add(item);
            }

            var vm = new BangKeHoachSVIndexViewModel
            {
                TenDot = dotHienTai.TenDot,
                HocKi = dotHienTai.IdHocKiNavigation?.MaHocKi,
                CoDot = true,
                DanhSachDeTai = deTaiItems
            };

            return View(vm);
        }

        /// <summary>
        /// Chi tiết kế hoạch của một đề tài
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Detail(int id, int page = 1)
        {
            var giangVien = await GetCurrentGiangVien();
            if (giangVien == null)
                return RedirectToAction("Index");

            var dotHienTai = await GetDotHienTai();
            if (dotHienTai == null)
                return RedirectToAction("Index");

            // Tìm đề tài - PHẢI TRONG ĐỢT HIỆN TẠI
            var deTai = await _context.DeTais
                .FirstOrDefaultAsync(dt => dt.Id == id && 
                                           dt.IdGvhd == giangVien.IdNguoiDung &&
                                           dt.IdDot == dotHienTai.Id);

            if (deTai == null)
                return RedirectToAction("Index");

            // Lấy danh sách sinh viên đã duyệt của đề tài TRONG ĐỢT NÀY
            var sinhVienDaDuyet = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(svdt => svdt.IdDeTai == id &&
                              (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"))
                .ToListAsync();

            var idSinhViens = sinhVienDaDuyet.Select(sv => sv.IdSinhVien).ToList();

            // Lấy kế hoạch công việc - CHỈ TRONG ĐỢT HIỆN TẠI
            var keHoachs = await _context.KeHoachCongViecs
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(k => idSinhViens.Contains(k.IdSinhVien) && k.IdDot == dotHienTai.Id)
                .OrderBy(k => k.Stt).ThenBy(k => k.Id)
                .ToListAsync();

            // Lấy file minh chứng
            var keHoachIds = keHoachs.Select(k => k.Id).ToList();
            var fileMinhChungs = await _context.FileMinhChungKeHoachs
                .Where(f => keHoachIds.Contains(f.IdKeHoach))
                .ToListAsync();

            // Thống kê theo trạng thái mới
            var tongCongViec = keHoachs.Count;
            var chuaThucHien = keHoachs.Count(k => string.IsNullOrEmpty(k.TrangThai) || 
                                                    k.TrangThai == "Chưa thực hiện" || 
                                                    k.TrangThai == "CHUA_THUC_HIEN");
            var dangThucHien = keHoachs.Count(k => k.TrangThai == "Đang thực hiện" || 
                                                    k.TrangThai == "DANG_THUC_HIEN");
            // Chờ duyệt = SV đã hoàn thành, chờ GV duyệt
            var choXacNhan = keHoachs.Count(k => k.TrangThai == "Chờ GV duyệt" || 
                                                  k.TrangThai == "CHO_DUYET" ||
                                                  k.TrangThai == "Hoàn thành");
            // Đã hoàn thành = GV đã duyệt
            var daHoanThanh = keHoachs.Count(k => k.TrangThai == "Đã duyệt" || k.TrangThai == "DA_DUYET");

            // Tính tiến độ
            double tienDo = 0;
            if (tongCongViec > 0)
            {
                var diemMoiTask = 100.0 / tongCongViec;
                foreach (var kh in keHoachs)
                {
                    if (kh.TrangThai == "Đã duyệt" || kh.TrangThai == "DA_DUYET")
                        tienDo += diemMoiTask;
                    else if (kh.TrangThai == "Chờ GV duyệt" || kh.TrangThai == "CHO_DUYET" || 
                             kh.TrangThai == "Hoàn thành")
                        tienDo += diemMoiTask * 0.75;
                    else if (kh.TrangThai == "Đang thực hiện" || kh.TrangThai == "DANG_THUC_HIEN")
                        tienDo += diemMoiTask * 0.5;
                }
            }

            var vm = new BangKeHoachSVDetailViewModel
            {
                IdDeTai = deTai.Id,
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                TenDot = dotHienTai.TenDot,
                TongCongViec = tongCongViec,
                ChuaThucHien = chuaThucHien,
                DangThucHien = dangThucHien,
                ChoXacNhan = choXacNhan,
                DaHoanThanh = daHoanThanh,
                TienDoTongQuat = Math.Round(tienDo, 1),
                DanhSachSinhVien = sinhVienDaDuyet.Select(svdt => new SinhVienKeHoachItem
                {
                    IdSinhVien = svdt.IdSinhVien ?? 0,
                    Mssv = svdt.IdSinhVienNavigation?.Mssv,
                    HoTen = svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    Email = svdt.IdSinhVienNavigation?.IdNguoiDungNavigation?.Email,
                    SoCongViec = keHoachs.Count(k => k.IdSinhVien == svdt.IdSinhVien),
                    TienDo = CalculateSinhVienProgress(keHoachs.Where(k => k.IdSinhVien == svdt.IdSinhVien).ToList())
                }).ToList(),
                DanhSachCongViec = keHoachs.Select(k => new CongViecKeHoachItem
                {
                    Id = k.Id,
                    Stt = k.Stt,
                    TenCongViec = k.TenCongViec,
                    MoTa = k.MoTaCongViec,
                    NguoiThucHien = k.NguoiPhuTrach ?? k.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    Mssv = k.IdSinhVienNavigation?.Mssv,
                    NgayBatDau = k.NgayBatDau?.ToString("dd/MM/yyyy"),
                    NgayKetThuc = k.NgayKetThuc?.ToString("dd/MM/yyyy"),
                    NgayBDThucTe = k.NgayBatDauThucTe?.ToString("dd/MM/yyyy"),
                    NgayKTThucTe = k.NgayKetThucThucTe?.ToString("dd/MM/yyyy"),
                    TrangThai = k.TrangThai,
                    StatusCss = GetStatusCss(k.TrangThai),
                    StatusText = GetStatusText(k.TrangThai),
                    GhiChu = k.GhiChu,
                    // Chờ duyệt khi SV đã hoàn thành hoặc gửi duyệt
                    CanDuyet = k.TrangThai == "Chờ GV duyệt" || 
                               k.TrangThai == "CHO_DUYET" || 
                               k.TrangThai == "Hoàn thành",
                    DanhSachMinhChung = fileMinhChungs
                        .Where(f => f.IdKeHoach == k.Id)
                        .Select(f => new FileMinhChungItem
                        {
                            Id = f.Id,
                            TenFile = f.TenFile,
                            DuongDan = f.DuongDan,
                            LoaiFile = f.LoaiFile,
                            NgayNop = f.NgayNop?.ToString("dd/MM/yyyy HH:mm")
                        }).ToList()
                }).ToPagedList(page, 10) // 10 items per page
            };

            return View(vm);
        }

        /// <summary>
        /// Chi tiết công việc (Trang xem/duyệt)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetailTask(int id)
        {
            var giangVien = await GetCurrentGiangVien();
            if (giangVien == null)
                return RedirectToAction("Index");

            var keHoach = await _context.KeHoachCongViecs
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(k => k.IdDotNavigation)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (keHoach == null)
                return RedirectToAction("Index");

            // Kiểm tra quyền: GV phải là GVHD của sinh viên này
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == keHoach.IdSinhVien &&
                                              svdt.IdDeTaiNavigation != null &&
                                              svdt.IdDeTaiNavigation.IdGvhd == giangVien.IdNguoiDung);

            if (svDeTai == null)
                return RedirectToAction("Index");

            // Lấy file minh chứng
            var fileMinhChungs = await _context.FileMinhChungKeHoachs
                .Where(f => f.IdKeHoach == id)
                .ToListAsync();

            var vm = new DetailTaskViewModel
            {
                Id = keHoach.Id,
                IdDeTai = svDeTai.IdDeTai ?? 0,
                Stt = keHoach.Stt,
                TenCongViec = keHoach.TenCongViec,
                MoTaCongViec = keHoach.MoTaCongViec,
                NguoiPhuTrach = keHoach.NguoiPhuTrach ?? keHoach.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                TenDot = keHoach.IdDotNavigation?.TenDot,
                NgayBatDau = keHoach.NgayBatDau?.ToString("yyyy-MM-dd"),
                NgayKetThuc = keHoach.NgayKetThuc?.ToString("yyyy-MM-dd"),
                NgayBatDauThucTe = keHoach.NgayBatDauThucTe?.ToString("yyyy-MM-dd"),
                NgayKetThucThucTe = keHoach.NgayKetThucThucTe?.ToString("yyyy-MM-dd"),
                TrangThai = keHoach.TrangThai,
                StatusCss = GetStatusCss(keHoach.TrangThai),
                StatusText = GetStatusText(keHoach.TrangThai),
                GhiChu = keHoach.GhiChu,
                CanDuyet = keHoach.TrangThai == "Chờ GV duyệt" || 
                           keHoach.TrangThai == "CHO_DUYET" || 
                           keHoach.TrangThai == "Hoàn thành",
                DanhSachFileMinhChung = fileMinhChungs.Select(f => new FileMinhChungItem
                {
                    Id = f.Id,
                    TenFile = f.TenFile,
                    DuongDan = f.DuongDan,
                    LoaiFile = f.LoaiFile,
                    NgayNop = f.NgayNop?.ToString("dd/MM/yyyy HH:mm")
                }).ToList()
            };

            return View(vm);
        }

        /// <summary>
        /// Lấy chi tiết công việc (API cho modal)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChiTietCongViec(int id)
        {
            var giangVien = await GetCurrentGiangVien();
            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            var keHoach = await _context.KeHoachCongViecs
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (keHoach == null)
                return Json(new { success = false, message = "Không tìm thấy công việc." });

            // Kiểm tra quyền
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == keHoach.IdSinhVien &&
                                              svdt.IdDeTaiNavigation != null &&
                                              svdt.IdDeTaiNavigation.IdGvhd == giangVien.IdNguoiDung);

            if (svDeTai == null)
                return Json(new { success = false, message = "Bạn không có quyền xem công việc này." });

            // Lấy file minh chứng
            var fileMinhChungs = await _context.FileMinhChungKeHoachs
                .Where(f => f.IdKeHoach == id)
                .ToListAsync();

            var data = new
            {
                id = keHoach.Id,
                stt = keHoach.Stt,
                tenCongViec = keHoach.TenCongViec,
                moTa = keHoach.MoTaCongViec,
                nguoiThucHien = keHoach.NguoiPhuTrach ?? keHoach.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                ngayBatDau = keHoach.NgayBatDau?.ToString("dd/MM/yyyy"),
                ngayKetThuc = keHoach.NgayKetThuc?.ToString("dd/MM/yyyy"),
                ngayBatDauThucTe = keHoach.NgayBatDauThucTe?.ToString("dd/MM/yyyy"),
                ngayKetThucThucTe = keHoach.NgayKetThucThucTe?.ToString("dd/MM/yyyy"),
                trangThai = keHoach.TrangThai,
                statusText = GetStatusText(keHoach.TrangThai),
                statusCss = GetStatusCss(keHoach.TrangThai),
                ghiChu = keHoach.GhiChu,
                canDuyet = keHoach.TrangThai == "Chờ GV duyệt" || 
                           keHoach.TrangThai == "CHO_DUYET" || 
                           keHoach.TrangThai == "Hoàn thành",
                danhSachMinhChung = fileMinhChungs.Select(f => new
                {
                    id = f.Id,
                    tenFile = f.TenFile,
                    duongDan = f.DuongDan,
                    loaiFile = f.LoaiFile,
                    ngayNop = f.NgayNop?.ToString("dd/MM/yyyy HH:mm")
                }).ToList()
            };

            return Json(new { success = true, data });
        }

        /// <summary>
        /// Duyệt hoặc từ chối kế hoạch
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetKeHoach([FromBody] DuyetKeHoachRequest request)
        {
            if (request == null || request.Id <= 0)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            // Bắt buộc phải nhập nhận xét
            if (string.IsNullOrWhiteSpace(request.NhanXet))
                return Json(new { success = false, message = "Vui lòng nhập nhận xét trước khi duyệt công việc." });

            var giangVien = await GetCurrentGiangVien();
            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            var keHoach = await _context.KeHoachCongViecs
                .Include(k => k.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(k => k.Id == request.Id);

            if (keHoach == null)
                return Json(new { success = false, message = "Không tìm thấy kế hoạch." });

            // Kiểm tra quyền: GV phải là GVHD của sinh viên này
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == keHoach.IdSinhVien &&
                                              svdt.IdDeTaiNavigation != null &&
                                              svdt.IdDeTaiNavigation.IdGvhd == giangVien.IdNguoiDung);

            if (svDeTai == null)
                return Json(new { success = false, message = "Bạn không có quyền duyệt kế hoạch này." });

            // Kiểm tra trạng thái hợp lệ
            var trangThaiHopLe = new[] { "Chờ GV duyệt", "CHO_DUYET", "Hoàn thành" };
            if (!trangThaiHopLe.Contains(keHoach.TrangThai))
                return Json(new { success = false, message = "Công việc không ở trạng thái chờ duyệt." });

            if (request.Action == "approve")
            {
                keHoach.TrangThai = "Đã duyệt";
                keHoach.NgayKetThucThucTe = DateOnly.FromDateTime(DateTime.Now);
            }
            else if (request.Action == "reject")
            {
                keHoach.TrangThai = "Đang thực hiện"; // Trả về để sửa
            }

            keHoach.GhiChu = request.NhanXet;

            await _context.SaveChangesAsync();

            var message = request.Action == "approve" 
                ? "Duyệt công việc thành công!" 
                : "Đã yêu cầu sinh viên chỉnh sửa.";

            return Json(new { success = true, message });
        }

        #region Helper Methods

        private double CalculateSinhVienProgress(List<KeHoachCongViec> keHoachs)
        {
            if (!keHoachs.Any()) return 0;

            var tongCongViec = keHoachs.Count;
            var diemMoiTask = 100.0 / tongCongViec;
            double tienDo = 0;

            foreach (var kh in keHoachs)
            {
                if (kh.TrangThai == "Đã duyệt" || kh.TrangThai == "DA_DUYET")
                    tienDo += diemMoiTask;
                else if (kh.TrangThai == "Chờ GV duyệt" || kh.TrangThai == "CHO_DUYET" || 
                         kh.TrangThai == "Hoàn thành")
                    tienDo += diemMoiTask * 0.75;
                else if (kh.TrangThai == "Đang thực hiện" || kh.TrangThai == "DANG_THUC_HIEN")
                    tienDo += diemMoiTask * 0.5;
            }

            return Math.Round(tienDo, 1);
        }

        private static string GetStatusCss(string? trangThai) => trangThai switch
        {
            "Đã duyệt" or "DA_DUYET" => "status-completed",
            "Chờ GV duyệt" or "CHO_DUYET" or "Hoàn thành" => "status-waiting",
            "Đang thực hiện" or "DANG_THUC_HIEN" => "status-running",
            _ => "status-pending"
        };

        private static string GetStatusText(string? trangThai) => trangThai switch
        {
            "Đã duyệt" or "DA_DUYET" => "Đã hoàn thành",
            "Chờ GV duyệt" or "CHO_DUYET" or "Hoàn thành" => "Chờ duyệt",
            "Đang thực hiện" or "DANG_THUC_HIEN" => "Đang thực hiện",
            _ => "Chưa thực hiện"
        };

        #endregion
    }
}
