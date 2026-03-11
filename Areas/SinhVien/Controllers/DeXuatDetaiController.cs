using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller đề xuất đề tài cho sinh viên
    /// Kế thừa BaseSinhVienController để kiểm tra nguyện vọng đã duyệt
    /// </summary>
    public class DeXuatDeTaiController : BaseSinhVienController
    {
        public DeXuatDeTaiController(QuanLyDoAnTotNghiepContext context) : base(context)
        {
        }

        // GET: Danh sách đề tài + form đề xuất
        [HttpGet]
        public async Task<IActionResult> Index(int? page, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            var viewModel = new DeXuatDeTaiViewModel();

            // Lấy danh sách chuyên ngành
            viewModel.DanhSachChuyenNganh = await _context.ChuyenNganhs
                .Select(cn => new ChuyenNganhItem
                {
                    Id = cn.Id,
                    TenChuyenNganh = cn.TenChuyenNganh
                })
                .ToListAsync();

            // Lấy thông tin sinh viên đang đăng nhập
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien != null)
            {
                viewModel.IdNguoiDeXuat = sinhVien.IdNguoiDung;
                viewModel.MssvSinhVien1 = sinhVien.Mssv;
            }

            // Lấy đợt đề xuất hiện tại
            var dotHienTai = await GetDotDeXuatHienTai();
            if (dotHienTai != null)
            {
                viewModel.IdDot = dotHienTai.Id;
                viewModel.TenDot = dotHienTai.TenDot;
            }

            // Lấy danh sách đề tài của sinh viên
            if (sinhVien == null)
            {
                return View(viewModel);
            }

            var query = _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Where(dt => dt.IdNguoiDeXuat == sinhVien.IdNguoiDung)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dt =>
                    (dt.MaDeTai != null && dt.MaDeTai.Contains(searchString)) ||
                    (dt.TenDeTai != null && dt.TenDeTai.Contains(searchString)) ||
                    (dt.IdChuyenNganhNavigation != null && dt.IdChuyenNganhNavigation.TenChuyenNganh != null && dt.IdChuyenNganhNavigation.TenChuyenNganh.Contains(searchString)));
            }

            viewModel.DanhSachDeTai = query
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new DeTaiItem
                {
                    Id = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    TenChuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    TrangThai = dt.TrangThai,
                    TenNguoiDeXuat = dt.IdNguoiDeXuatNavigation != null ? dt.IdNguoiDeXuatNavigation.HoTen : ""
                })
                .ToPagedList(pageNumber, pageSize);

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
                    message = "Chưa đến giai đoạn đề xuất đề tài hoặc đã hết hạn. Vui lòng liên hệ phòng đào tạo để biết thêm chi tiết."
                });
            }

            // Lấy thông tin sinh viên
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });
            }

            // ============================================
            // BUSINESS RULE: Bắt buộc chọn GVHD khi SV đề xuất
            // ============================================
            if (!model.IdGvhd.HasValue || model.IdGvhd.Value <= 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn Giảng viên hướng dẫn. Trường này là bắt buộc." });
            }

            // ============================================
            // BUSINESS RULE #1: Ràng buộc 1 SV - 1 Đề tài
            // ============================================
            var svDaCoDeTai = await _context.SinhVienDeTais
                .AnyAsync(svdt => svdt.IdSinhVien == sinhVien.IdNguoiDung 
                    && svdt.IdDeTaiNavigation!.IdDot == dotDoAn.Id);
            if (svDaCoDeTai)
            {
                return Json(new { success = false, message = "Bạn đã có đề tài trong đợt này, không thể đề xuất thêm." });
            }

            // Kiểm tra sinh viên thứ 2 (nếu có)
            DATN_TMS.Models.SinhVien? sinhVien2 = null;
            if (!string.IsNullOrEmpty(model.MssvSinhVien2))
            {
                sinhVien2 = await _context.SinhViens
                    .FirstOrDefaultAsync(sv => sv.Mssv == model.MssvSinhVien2);

                if (sinhVien2 is not null)
                {
                    var sv2DaCoDeTai = await _context.SinhVienDeTais
                        .AnyAsync(svdt => svdt.IdSinhVien == sinhVien2.IdNguoiDung 
                            && svdt.IdDeTaiNavigation!.IdDot == dotDoAn.Id);
                    if (sv2DaCoDeTai)
                    {
                        return Json(new { success = false, message = $"Sinh viên {model.MssvSinhVien2} đã có đề tài, không thể thêm vào." });
                    }
                }
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
                // ============================================
                // BUSINESS RULE #2: SV chọn GVHD => tự động phân công và duyệt
                // ============================================
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
                    IdNguoiDeXuat = sinhVien.IdNguoiDung,
                    IdGvhd = model.IdGvhd, // Bắt buộc có GVHD
                    IdDot = dotDoAn.Id,
                    TrangThai = "CHO_DUYET"
                };

                _context.DeTais.Add(deTai);
                await _context.SaveChangesAsync();

                // Thêm sinh viên vào đề tài
                // Nếu đã chọn GVHD => SV tự động được duyệt (TrangThai = "DA_DUYET" hoặc "Đã duyệt")
                var trangThaiSV = model.IdGvhd.HasValue ? "DA_DUYET" : "CHO_DUYET";

                var svDeTai1 = new SinhVienDeTai
                {
                    IdDeTai = deTai.Id,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    TrangThai = trangThaiSV,
                    NgayDangKy = DateTime.Now
                };
                _context.SinhVienDeTais.Add(svDeTai1);

                // Nếu có sinh viên thứ 2
                if (sinhVien2 is not null)
                {
                    var svDeTai2 = new SinhVienDeTai
                    {
                        IdDeTai = deTai.Id,
                        IdSinhVien = sinhVien2.IdNguoiDung,
                        TrangThai = trangThaiSV,
                        NgayDangKy = DateTime.Now
                    };
                    _context.SinhVienDeTais.Add(svDeTai2);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đề xuất đề tài thành công! Vui lòng chờ xét duyệt."
                });
            }
            catch (Exception ex)
            {
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

        // API tìm kiếm GVHD theo từ khóa (Mã GV hoặc Họ tên)
        // Tìm tất cả giảng viên (không giới hạn chỉ GV đã đề xuất đề tài)
        [HttpGet]
        public async Task<IActionResult> TimKiemGVHD(string keyword)
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

            // Tìm tất cả giảng viên theo từ khóa
            var danhSachGv = await _context.GiangViens
                .Include(gv => gv.IdNguoiDungNavigation)
                .Include(gv => gv.IdBoMonNavigation)
                .Where(gv => gv.MaGv!.ToLower().Contains(keyword)
                    || gv.IdNguoiDungNavigation!.HoTen!.ToLower().Contains(keyword))
                .Take(10)
                .Select(gv => new
                {
                    idGiangVien = gv.IdNguoiDung,
                    maGv = gv.MaGv,
                    hoTen = gv.IdNguoiDungNavigation!.HoTen,
                    hocVi = gv.HocVi,
                    tenBoMon = gv.IdBoMonNavigation != null ? gv.IdBoMonNavigation.TenBoMon : ""
                })
                .ToListAsync();

            return Json(new { success = true, data = danhSachGv });
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