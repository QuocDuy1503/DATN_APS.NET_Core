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
            }

            // Lấy danh sách đề tài của giảng viên (GVHD hoặc người đề xuất)
            var idNguoiDung = giangVien.IdNguoiDung;
            viewModel.DanhSachDeTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Where(dt => dt.IdGvhd == idNguoiDung || dt.IdNguoiDeXuat == idNguoiDung)
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
                    IdNguoiDeXuat = giangVien.IdNguoiDung,
                    IdGvhd = giangVien.IdNguoiDung, // Giảng viên là GVHD của chính đề tài mình đề xuất
                    IdDot = dotDoAn.Id,
                    TrangThai = "CHO_DUYET"
                };

                _context.DeTais.Add(deTai);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đề xuất đề tài thành công! Vui lòng chờ xét duyệt."
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
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

        #endregion
    }
}
