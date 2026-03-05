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

        // Ki?m tra ??ng nh?p
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

        // GET: Danh sách ?? tài ?? xu?t c?a gi?ng viên
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DeXuatDeTaiViewModel();

            // L?y danh sách chuyên ngành
            viewModel.DanhSachChuyenNganh = await _context.ChuyenNganhs
                .Select(cn => new ChuyenNganhItem
                {
                    Id = cn.Id,
                    TenChuyenNganh = cn.TenChuyenNganh
                })
                .ToListAsync();

            // L?y thông tin gi?ng viên ?ang ??ng nh?p
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien != null)
            {
                viewModel.IdNguoiDeXuat = giangVien.IdNguoiDung;
                viewModel.MaGVDeXuat = giangVien.MaGv;
                viewModel.TenGVDeXuat = giangVien.IdNguoiDungNavigation?.HoTen;
            }

            // L?y ??t ?? xu?t hi?n t?i
            var dotHienTai = await GetDotDeXuatHienTai();
            if (dotHienTai != null)
            {
                viewModel.IdDot = dotHienTai.Id;
                viewModel.TenDot = dotHienTai.TenDot;
            }

            // L?y danh sách ?? tài c?a gi?ng viên (GVHD ho?c ng??i ?? xu?t)
            viewModel.DanhSachDeTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung || dt.IdNguoiDeXuat == giangVien.IdNguoiDung)
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

        // POST: ?? xu?t ?? tài m?i
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeXuatDeTaiViewModel model)
        {
            // Ki?m tra ??t ?? xu?t
            var dotDoAn = await GetDotDeXuatHienTai();
            if (dotDoAn == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Ch?a ??n giai ?o?n ?? xu?t ?? tài ho?c ?ã h?t h?n. Vui lòng liên h? phòng ?ào t?o ?? bi?t thêm chi ti?t."
                });
            }

            // L?y thông tin gi?ng viên
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens
                .FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                return Json(new { success = false, message = "Không tìm th?y thông tin gi?ng viên." });
            }

            // Ki?m tra mã ?? tài ?ã t?n t?i ch?a
            var maDeTaiTonTai = await _context.DeTais
                .AnyAsync(dt => dt.MaDeTai == model.MaDeTai && dt.IdDot == dotDoAn.Id);

            if (maDeTaiTonTai)
            {
                return Json(new { success = false, message = "Mã ?? tài ?ã t?n t?i trong ??t này." });
            }

            try
            {
                // T?o ?? tài m?i
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
                    IdGvhd = giangVien.IdNguoiDung, // Gi?ng viên là GVHD c?a chính ?? tài mình ?? xu?t
                    IdDot = dotDoAn.Id,
                    TrangThai = "CHO_DUYET"
                };

                _context.DeTais.Add(deTai);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "?? xu?t ?? tài thành công! Vui lòng ch? xét duy?t."
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có l?i x?y ra. Vui lòng th? l?i sau." });
            }
        }

        // GET: Chi ti?t ?? tài
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

        // POST: C?p nh?t ?? tài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(DeXuatDeTaiViewModel model)
        {
            // Ki?m tra ??t ?? xu?t
            var dotDoAn = await GetDotDeXuatHienTai();
            if (dotDoAn == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Ch?a ??n giai ?o?n ?? xu?t ?? tài ho?c ?ã h?t h?n."
                });
            }

            var deTai = await _context.DeTais.FindAsync(model.Id);
            if (deTai == null)
            {
                return Json(new { success = false, message = "Không tìm th?y ?? tài." });
            }

            // Ch? cho phép s?a n?u còn ? tr?ng thái "CHO_DUYET"
            if (deTai.TrangThai != "CHO_DUYET")
            {
                return Json(new { success = false, message = "Không th? s?a ?? tài ?ã ???c duy?t ho?c t? ch?i." });
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

                return Json(new { success = true, message = "C?p nh?t ?? tài thành công!" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có l?i x?y ra. Vui lòng th? l?i sau." });
            }
        }

        // API ki?m tra tr?ng thái ?? xu?t
        [HttpGet]
        public async Task<IActionResult> KiemTraTrangThai()
        {
            var dotDoAn = await GetDotDeXuatHienTai();

            if (dotDoAn == null)
            {
                return Json(new
                {
                    dangMoDeXuat = false,
                    message = "Hi?n t?i ch?a có ??t ?? xu?t ?? tài nào ?ang m?."
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

        // L?y ??t ?ang trong giai ?o?n ?? xu?t ?? tài
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
