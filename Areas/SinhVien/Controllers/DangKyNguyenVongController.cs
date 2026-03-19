using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller đăng ký nguyện vọng cho sinh viên
    /// BỎ QUA kiểm tra nguyện vọng vì đây là nơi SV đăng ký nguyện vọng
    /// </summary>
    public class DangKyNguyenVongController : BaseSinhVienController
    {
        public DangKyNguyenVongController(QuanLyDoAnTotNghiepContext context) : base(context)
        {
        }

        /// <summary>
        /// Bỏ qua kiểm tra nguyện vọng cho controller này
        /// </summary>
        protected override bool BoQuaKiemTraNguyenVong => true;

        // GET: Hiển thị form đăng ký
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DangKyNguyenVongViewModel();

            // Lấy danh sách chuyên ngành cho dropdown
            viewModel.DanhSachChuyenNganh = await _context.ChuyenNganhs
                .Select(cn => new ChuyenNganhItem
                {
                    Id = cn.Id,
                    TenChuyenNganh = cn.TenChuyenNganh
                })
                .ToListAsync();

            // Tự động điền thông tin sinh viên đang đăng nhập
            var mssv = HttpContext.Session.GetString("UserCode");
            if (!string.IsNullOrEmpty(mssv))
            {
                var sinhVien = await _context.SinhViens
                    .Include(sv => sv.IdNguoiDungNavigation)
                    .Include(sv => sv.IdChuyenNganhNavigation)
                    .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

                if (sinhVien != null)
                {
                    viewModel.IdSinhVien = sinhVien.IdNguoiDung;
                    viewModel.Mssv = sinhVien.Mssv;
                    viewModel.HoTen = sinhVien.IdNguoiDungNavigation?.HoTen;
                    viewModel.Email = sinhVien.IdNguoiDungNavigation?.Email;
                    viewModel.Sdt = sinhVien.IdNguoiDungNavigation?.Sdt;
                    viewModel.IdChuyenNganh = sinhVien.IdChuyenNganh;
                    viewModel.TenChuyenNganh = sinhVien.IdChuyenNganhNavigation?.TenChuyenNganh;
                    viewModel.SoTinChiTichLuy = sinhVien.TinChiTichLuy;

                    // Lấy GPA
                    viewModel.Gpa = await TinhGPA(sinhVien.IdNguoiDung);

                    // Kiểm tra điều kiện tốt nghiệp
                    viewModel.DieuKienXetDuyet = await KiemTraDieuKienTotNghiep(sinhVien.IdNguoiDung);
                }
            }

            // Lấy đợt đăng ký hiện tại
            var dotHienTai = await GetDotDangKyHienTai();
            if (dotHienTai != null)
            {
                viewModel.IdDot = dotHienTai.Id;
                viewModel.TenDot = dotHienTai.TenDot;
            }

            return View(viewModel);
        }

        // POST: Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DangKyNguyenVongViewModel model)
        {
            // Kiểm tra đợt đăng ký trước
            var dotDoAn = await GetDotDangKyHienTai();
            if (dotDoAn == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Chưa đến giai đoạn đăng ký nguyện vọng hoặc đã hết hạn đăng ký. Vui lòng liên hệ phòng đào tạo để biết thêm chi tiết."
                });
            }

            // Lấy thông tin sinh viên
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });
            }

            // Kiểm tra đã đăng ký chưa
            var daDangKy = await _context.DangKyNguyenVongs
                .AnyAsync(dk => dk.IdSinhVien == sinhVien.IdNguoiDung && dk.IdDot == dotDoAn.Id);

            if (daDangKy)
            {
                return Json(new { success = false, message = "Bạn đã đăng ký nguyện vọng cho đợt này rồi." });
            }

            try
            {
                // Tạo bản ghi đăng ký mới
                var dangKy = new DangKyNguyenVong
                {
                    IdDot = dotDoAn.Id,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    SoTinChiTichLuyHienTai = (int?)model.SoTinChiTichLuy,
                    TrangThai = 0, // 0: Chờ xét duyệt
                    NgayDangKy = DateTime.Now
                };

                // Cập nhật thông tin sinh viên
                sinhVien.TinChiTichLuy = model.SoTinChiTichLuy;
                if (model.IdChuyenNganh.HasValue)
                {
                    sinhVien.IdChuyenNganh = model.IdChuyenNganh;
                }

                // Cập nhật SĐT
                if (sinhVien.IdNguoiDungNavigation != null && !string.IsNullOrEmpty(model.Sdt))
                {
                    sinhVien.IdNguoiDungNavigation.Sdt = model.Sdt;
                }

                _context.DangKyNguyenVongs.Add(dangKy);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Đăng ký nguyện vọng đồ án thành công! Vui lòng chờ xét duyệt từ khoa/giảng viên hướng dẫn."
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại sau." });
            }
        }

        #region Private Methods

        // Lấy đợt đang trong giai đoạn đăng ký nguyện vọng
        private async Task<DotDoAn?> GetDotDangKyHienTai()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var dotDoAn = await _context.DotDoAns
                .Where(d => d.TrangThai == true
                    && d.NgayBatDauDkNguyenVong <= today
                    && d.NgayKetThucDkNguyenVong >= today)
                .FirstOrDefaultAsync();

            return dotDoAn;
        }

        // Tính GPA từ bảng KetQuaHocTap
        private async Task<double?> TinhGPA(int idSinhVien)
        {
            var ketQua = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == idSinhVien)
                .OrderByDescending(kq => kq.Id)
                .FirstOrDefaultAsync();

            return ketQua?.Gpa;  
        }

        /// <summary>
        /// Kiểm tra điều kiện tốt nghiệp dựa trên kết quả học tập và CTDT
        /// - Đại cương >= 60 TC
        /// - Cơ sở ngành: BB >= 18, TC >= 18
        /// - Chuyên ngành: tổng >= 18 (cho phép thiếu 3 TC)
        /// </summary>
        private async Task<DieuKienXetDuyetResult> KiemTraDieuKienTotNghiep(int idSinhVien)
        {
            var result = new DieuKienXetDuyetResult();

            var sinhVien = await _context.SinhViens.FindAsync(idSinhVien);
            if (sinhVien == null) return result;

            // Tìm CTDT theo khóa học của sinh viên
            var ctdt = await _context.ChuongTrinhDaoTaos
                .FirstOrDefaultAsync(c => c.IdKhoaHoc == sinhVien.IdKhoaHoc && c.TrangThai == true);

            if (ctdt == null)
            {
                result.ThongBaoLoi.Add("Không tìm thấy chương trình đào tạo cho sinh viên.");
                return result;
            }

            // Lấy chi tiết CTDT kèm khối kiến thức
            var chiTietList = await _context.ChiTietCtdts
                .Include(ct => ct.IdKhoiKienThucNavigation)
                .Where(ct => ct.IdCtdt == ctdt.Id)
                .ToListAsync();

            // Tạo bản đồ: mã học phần → (tên khối, loại HP)
            var courseMap = new Dictionary<string, (string TenKhoi, string LoaiHP)>(StringComparer.OrdinalIgnoreCase);
            foreach (var ct in chiTietList)
            {
                string code = ct.MaHocPhan?.Trim() ?? "";
                if (!string.IsNullOrEmpty(code) && !courseMap.ContainsKey(code))
                {
                    courseMap[code] = (
                        ct.IdKhoiKienThucNavigation?.TenKhoi ?? "",
                        ct.LoaiHocPhan ?? ""
                    );
                }
            }

            // Lấy các môn đã đạt
            var passedCourses = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == idSinhVien && kq.KetQua == true)
                .ToListAsync();

            double tcDaiCuong = 0;
            double tcCoSoBB = 0, tcCoSoTC = 0;
            double tcCNBB = 0, tcCNTC = 0;

            foreach (var kq in passedCourses)
            {
                string maHP = kq.MaHocPhan?.Trim() ?? "";
                if (string.IsNullOrEmpty(maHP) || !courseMap.TryGetValue(maHP, out var info))
                    continue;

                string khoi = info.TenKhoi.ToLower();
                string loai = info.LoaiHP.ToLower();
                double tc = kq.SoTc ?? 0;

                if (khoi.Contains("đại cương"))
                {
                    tcDaiCuong += tc;
                }
                else if (khoi.Contains("cơ sở"))
                {
                    if (loai.Contains("bắt buộc")) tcCoSoBB += tc;
                    else tcCoSoTC += tc;
                }
                else if (khoi.Contains("chuyên ngành"))
                {
                    if (loai.Contains("bắt buộc")) tcCNBB += tc;
                    else tcCNTC += tc;
                }
                else if (khoi.Contains("tự chọn"))
                {
                    // Khối tự chọn chung → tính vào chuyên ngành TC
                    tcCNTC += tc;
                }
            }

            result.TongTinChiDaiCuong = tcDaiCuong;
            result.TongTinChiCoSoNganhBB = tcCoSoBB;
            result.TongTinChiCoSoNganhTC = tcCoSoTC;
            result.TongTinChiChuyenNganh = tcCNBB + tcCNTC;

            // Kiểm tra điều kiện
            if (tcDaiCuong < 60)
                result.ThongBaoLoi.Add($"Thiếu {60 - tcDaiCuong} tín chỉ đại cương (hiện có: {tcDaiCuong}/60)");

            if (tcCoSoBB < 18)
                result.ThongBaoLoi.Add($"Thiếu {18 - tcCoSoBB} tín chỉ cơ sở ngành bắt buộc (hiện có: {tcCoSoBB}/18)");

            if (tcCoSoTC < 18)
                result.ThongBaoLoi.Add($"Thiếu {18 - tcCoSoTC} tín chỉ cơ sở ngành tự chọn (hiện có: {tcCoSoTC}/18)");

            // Chuyên ngành: cho phép thiếu 3 TC (21 - 3 = 18)
            double totalCN = tcCNBB + tcCNTC;
            if (totalCN < 18)
                result.ThongBaoLoi.Add($"Thiếu tín chỉ chuyên ngành (hiện có: {totalCN}/18 tối thiểu)");

            result.IsEligible = result.ThongBaoLoi.Count == 0;
            return result;
        }

        #endregion
    }
}