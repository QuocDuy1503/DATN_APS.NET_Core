using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class DeXuatDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public DeXuatDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // Kiểm tra đăng nhập
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

        // GET: Danh sách đề tài đề xuất của giảng viên
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DeXuatDeTaiViewModel();

            // Lấy danh sách chuyên ngành
            viewModel.DanhSachChuyenNganh = await _context.ChuyenNganhs
                .Select(cn => new ChuyenNganhItem
                {
                    Id = cn.Id,
                    TenChuyenNganh = cn.TenChuyenNganh
                })
                .ToListAsync();

            // Lấy thông tin giảng viên đang đăng nhập
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                return View(viewModel);
            }

            viewModel.IdNguoiDeXuat = giangVien.IdNguoiDung;
            viewModel.MaGVDeXuat = giangVien.MaGv;
            viewModel.TenGVDeXuat = giangVien.IdNguoiDungNavigation?.HoTen;

            // Lấy đợt đề xuất hiện tại
            var dotHienTai = await GetDotDeXuatHienTai();
            if (dotHienTai != null)
            {
                viewModel.IdDot = dotHienTai.Id;
                viewModel.TenDot = dotHienTai.TenDot;
                // Không load sẵn danh sách SV nữa - sẽ tìm kiếm động qua API
            }

            // Lấy danh sách đề tài do giảng viên đề xuất
            var idNguoiDung = giangVien.IdNguoiDung;
            viewModel.DanhSachDeTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Where(dt => dt.IdNguoiDeXuat == idNguoiDung)
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new DeTaiItem
                {
                    Id = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    TenChuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    TrangThai = dt.TrangThai,
                    TenGVDeXuat = dt.IdNguoiDeXuatNavigation != null ? dt.IdNguoiDeXuatNavigation.HoTen : ""
                })
                .ToListAsync();

            return View(viewModel);
        }

        // POST: Đề xuất đề tài mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeXuatDeTaiViewModel model)
        {
            // Kiểm tra đợt đề xuất
            var dotDoAn = await GetDotDeXuatHienTai();
            if (dotDoAn == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Chưa đến giai đoạn đề xuất đề tài hoặc đã hết hạn. Vui lòng liên hệ phòng Đào tạo để biết thêm chi tiết."
                });
            }

            // Lấy thông tin giảng viên
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });
            }

            // Kiểm tra mã đề tài đã tồn tại chưa
            var maDeTaiTonTai = await _context.DeTais
                .AnyAsync(dt => dt.MaDeTai == model.MaDeTai && dt.IdDot == dotDoAn.Id);

            if (maDeTaiTonTai)
            {
                return Json(new { success = false, message = "Mã đề tài đã tồn tại trong đợt này." });
            }

            // ============================================
            // BUSINESS RULE #1: Ràng buộc 1 SV - 1 Đề tài
            // Kiểm tra trước khi tạo đề tài
            // ============================================
            var sinhVienIds = new List<int>();
            if (model.IdSinhVien1.HasValue && model.IdSinhVien1.Value > 0)
                sinhVienIds.Add(model.IdSinhVien1.Value);
            if (model.IdSinhVien2.HasValue && model.IdSinhVien2.Value > 0 && model.IdSinhVien2.Value != model.IdSinhVien1)
                sinhVienIds.Add(model.IdSinhVien2.Value);

            foreach (var idSv in sinhVienIds)
            {
                var svDaCoDeTai = await _context.SinhVienDeTais
                    .Include(svdt => svdt.IdSinhVienNavigation)
                    .AnyAsync(svdt => svdt.IdSinhVien == idSv 
                        && svdt.IdDeTaiNavigation!.IdDot == dotDoAn.Id);

                if (svDaCoDeTai)
                {
                    var svInfo = await _context.SinhViens
                        .Include(sv => sv.IdNguoiDungNavigation)
                        .FirstOrDefaultAsync(sv => sv.IdNguoiDung == idSv);
                    var tenSv = svInfo?.IdNguoiDungNavigation?.HoTen ?? svInfo?.Mssv ?? idSv.ToString();
                    return Json(new { success = false, message = $"Sinh viên '{tenSv}' đã có đề tài, không thể thực hiện thao tác." });
                }
            }

            try
            {
                // Tạo đề tài mới
                var deTai = new DeTai
                {
                    MaDeTai = model.MaDeTai,
                    TenDeTai = model.TenDeTai,
                    IdChuyenNganh = model.IdChuyenNganh,
                    MucTieuChinh = model.MucTieuChinh,
                    PhamViChucNang = model.PhamViChucNang,
                    CongNgheSuDung = model.CongNgheSuDung,
                    YeuCauTinhMoi = model.YeuCauTinhMoi,
                    SanPhamKetQuaDuKien = model.SanPhamKetQuaDuKien,
                    NhiemVuCuThe = model.NhiemVuCuThe,
                    IdNguoiDeXuat = giangVien.IdNguoiDung,
                    IdGvhd = giangVien.IdNguoiDung, // Giảng viên là GVHD của chính đề tài mình đề xuất
                    IdDot = dotDoAn.Id,
                    TrangThai = "CHO_DUYET"
                };

                _context.DeTais.Add(deTai);
                await _context.SaveChangesAsync();

                // Thêm sinh viên vào đề tài (nếu có chọn)
                foreach (var idSv in sinhVienIds)
                {
                    // Kiểm tra sinh viên có hợp lệ không (đã đăng ký nguyện vọng được duyệt)
                    var isValidSv = await _context.DangKyNguyenVongs
                        .AnyAsync(dk => dk.IdSinhVien == idSv && dk.IdDot == dotDoAn.Id && dk.TrangThai == 1);

                    if (isValidSv)
                    {
                        var svDeTai = new SinhVienDeTai
                        {
                            IdDeTai = deTai.Id,
                            IdSinhVien = idSv,
                            TrangThai = "DA_DUYET", // GV đề xuất nên tự động duyệt SV
                            NgayDangKy = DateTime.Now
                        };
                        _context.SinhVienDeTais.Add(svDeTai);
                    }
                }

                if (sinhVienIds.Any())
                {
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = "Đề xuất đề tài thành công! Vui lòng chờ xét duyệt."
                });
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết để debug
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = $"Có lỗi xảy ra: {errorMessage}" });
            }
        }

        // GET: Chi tiết đề tài
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var deTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id);

            if (deTai == null)
            {
                return NotFound();
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    deTai.Id,
                    deTai.MaDeTai,
                    deTai.TenDeTai,
                    deTai.IdChuyenNganh,
                    deTai.MucTieuChinh,
                    deTai.PhamViChucNang,
                    deTai.CongNgheSuDung,
                    deTai.YeuCauTinhMoi,
                    deTai.SanPhamKetQuaDuKien,
                    deTai.NhiemVuCuThe,
                    deTai.TrangThai,
                    TenChuyenNganh = deTai.IdChuyenNganhNavigation?.TenChuyenNganh,
                    SinhViens = deTai.SinhVienDeTais.Select(sv => new
                    {
                        Mssv = sv.IdSinhVienNavigation?.Mssv,
                        HoTen = sv.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen
                    })
                }
            });
        }

        // POST: Cập nhật đề tài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DeXuatDeTaiViewModel model)
        {
            // Kiểm tra đợt đề xuất
            var dotDoAn = await GetDotDeXuatHienTai();
            if (dotDoAn == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Chưa đến giai đoạn đề xuất đề tài hoặc đã hết hạn."
                });
            }

            var deTai = await _context.DeTais.FindAsync(model.Id);
            if (deTai == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đề tài." });
            }

            // Chỉ cho phép sửa nếu còn ở trạng thái "CHO_DUYET"
            if (deTai.TrangThai != "CHO_DUYET")
            {
                return Json(new { success = false, message = "Không thể sửa đề tài đã được duyệt hoặc từ chối." });
            }

            try
            {
                deTai.MaDeTai = model.MaDeTai;
                deTai.TenDeTai = model.TenDeTai;
                deTai.IdChuyenNganh = model.IdChuyenNganh;
                deTai.MucTieuChinh = model.MucTieuChinh;
                deTai.PhamViChucNang = model.PhamViChucNang;
                deTai.CongNgheSuDung = model.CongNgheSuDung;
                deTai.YeuCauTinhMoi = model.YeuCauTinhMoi;
                deTai.SanPhamKetQuaDuKien = model.SanPhamKetQuaDuKien;
                deTai.NhiemVuCuThe = model.NhiemVuCuThe;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật đề tài thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }

        // API kiểm tra trạng thái đề xuất
        [HttpGet]
        public async Task<IActionResult> KiemTraTrangThai()
        {
            var dotDoAn = await GetDotDeXuatHienTai();

            if (dotDoAn == null)
            {
                return Json(new
                {
                    dangMoDeXuat = false,
                    message = "Hiện tại chưa có đợt đề xuất đề tài nào đang mở."
                });
            }

            return Json(new
            {
                dangMoDeXuat = true,
                tenDot = dotDoAn.TenDot,
                ngayBatDau = dotDoAn.NgayBatDauDeXuatDeTai,
                ngayKetThuc = dotDoAn.NgayKetThucDeXuatDeTai
            });
        }

        // API tìm kiếm sinh viên theo từ khóa (MSSV hoặc Họ tên)
        [HttpGet]
        public async Task<IActionResult> TimKiemSinhVien(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return Json(new { success = true, data = new List<object>() });
            }

            var dotDoAn = await GetDotDeXuatHienTai();
            if (dotDoAn == null)
            {
                return Json(new { success = false, message = "Không có đợt đề xuất đang mở." });
            }

            keyword = keyword.Trim().ToLower();

            // Tìm sinh viên đã đăng ký nguyện vọng được duyệt trong đợt hiện tại
            var danhSachSv = await _context.DangKyNguyenVongs
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdChuyenNganhNavigation)
                .Where(dk => dk.IdDot == dotDoAn.Id 
                    && dk.TrangThai == 1
                    && (dk.IdSinhVienNavigation!.Mssv!.ToLower().Contains(keyword)
                        || dk.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen!.ToLower().Contains(keyword)))
                .Take(10)
                .Select(dk => new
                {
                    idSinhVien = dk.IdSinhVien,
                    mssv = dk.IdSinhVienNavigation!.Mssv,
                    hoTen = dk.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen,
                    tenChuyenNganh = dk.IdSinhVienNavigation.IdChuyenNganhNavigation != null
                        ? dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh
                        : ""
                })
                .ToListAsync();

            return Json(new { success = true, data = danhSachSv });
        }

        #region Private Methods

        // Lấy đợt đang trong giai đoạn đề xuất đề tài
        private async Task<DotDoAn?> GetDotDeXuatHienTai()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var dotDoAn = await _context.DotDoAns
                .Where(d => d.TrangThai == true
                    && d.NgayBatDauDeXuatDeTai <= today
                    && d.NgayKetThucDeXuatDeTai >= today)
                .FirstOrDefaultAsync();

            return dotDoAn;
        }

        // Lấy danh sách sinh viên đã đăng ký nguyện vọng được duyệt trong đợt
        private async Task<List<SinhVienDuocChonItem>> GetSinhVienDuocDuyetNguyenVong(int idDot)
        {
            // TrangThai = 1 nghĩa là "Đạt" (đã được duyệt)
            var danhSachSv = await _context.DangKyNguyenVongs
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdChuyenNganhNavigation)
                .Where(dk => dk.IdDot == idDot && dk.TrangThai == 1)
                .Select(dk => new SinhVienDuocChonItem
                {
                    IdSinhVien = dk.IdSinhVien ?? 0,
                    Mssv = dk.IdSinhVienNavigation!.Mssv,
                    HoTen = dk.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen,
                    TenChuyenNganh = dk.IdSinhVienNavigation.IdChuyenNganhNavigation != null 
                        ? dk.IdSinhVienNavigation.IdChuyenNganhNavigation.TenChuyenNganh 
                        : ""
                })
                .ToListAsync();

            return danhSachSv;
        }

        #endregion
    }
}
