using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller nộp báo cáo cho sinh viên
    /// Kế thừa BaseSinhVienController để kiểm tra nguyện vọng đã duyệt
    /// </summary>
    public class NopBaoCaoController : BaseSinhVienController
    {
        private readonly IWebHostEnvironment _env;

        public NopBaoCaoController(QuanLyDoAnTotNghiepContext context, IWebHostEnvironment env) : base(context)
        {
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null)
                return View(new NopBaoCaoIndexViewModel { ThongBaoDot = "Không tìm thấy thông tin sinh viên." });

            // Tìm đợt đang mở hiện tại (chưa kết thúc)
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dot = await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .Where(d => d.TrangThai == true && 
                           (!d.NgayKetThucDot.HasValue || d.NgayKetThucDot.Value >= today))
                .OrderByDescending(d => d.NgayBatDauDot)
                .FirstOrDefaultAsync();

            if (dot == null)
            {
                return View(new NopBaoCaoIndexViewModel 
                { 
                    CoDot = false,
                    ThongBaoDot = "Không tìm thấy đợt đồ án đang mở. Vui lòng liên hệ Ban chủ nhiệm khoa." 
                });
            }

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            // Lấy báo cáo của nhóm (tất cả sinh viên cùng đề tài)
            var dsSinhVienCungDeTai = await GetDanhSachIdSinhVienCungDeTai(sinhVien.IdNguoiDung, dot.Id);

            var baoCaos = await _context.BaoCaoNops
                .Where(b => dsSinhVienCungDeTai.Contains(b.IdSinhVien ?? 0) &&
                            b.IdDot == dot.Id &&
                            b.LoaiBaoCao != null &&
                            b.LoaiBaoCao != "MINH_CHUNG")
                .ToListAsync();

            var tenDot = dot.TenDot ?? "---";
            var hocKi = dot.IdHocKiNavigation?.MaHocKi;

            // Tạo thông báo đợt
            var thongBaoDot = $"Đợt đồ án: {tenDot}";
            if (!string.IsNullOrEmpty(hocKi))
                thongBaoDot += $" | Học kỳ: {hocKi}";
            if (dot.NgayBatDauDot.HasValue && dot.NgayKetThucDot.HasValue)
                thongBaoDot += $" | Thời gian: {dot.NgayBatDauDot.Value:dd/MM/yyyy} - {dot.NgayKetThucDot.Value:dd/MM/yyyy}";

            var vm = new NopBaoCaoIndexViewModel
            {
                TenDot = tenDot,
                HocKi = hocKi,
                CoDot = true,
                ThongBaoDot = thongBaoDot,
                DeCuong = BuildBoxItem(baoCaos, "DE_CUONG", "Nộp đề cương", dot.NgayBatDauNopDeCuong, dot.NgayKetThucNopDeCuong, today),
                GiuaKy = BuildBoxItem(baoCaos, "GIUA_KY", "Nộp báo cáo giữa kỳ", dot.NgayBatDauBaoCaoGiuaKi, dot.NgayKetThucBaoCaoGiuaKi, today),
                CuoiKy = BuildBoxItem(baoCaos, "CUOI_KY", "Nộp báo cáo cuối kỳ", dot.NgayBatDauBaoCaoCuoiKi, dot.NgayKetThucBaoCaoCuoiKi, today)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string loai)
        {
            if (string.IsNullOrEmpty(loai) || !new[] { "DE_CUONG", "GIUA_KY", "CUOI_KY" }.Contains(loai))
                return RedirectToAction("Index");

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .Include(sv => sv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);

            if (sinhVien == null) return RedirectToAction("Index");

            // Tìm đợt đang mở (chưa kết thúc)
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dot = await _context.DotDoAns
                .Include(d => d.IdHocKiNavigation)
                .Where(d => d.TrangThai == true && 
                           (!d.NgayKetThucDot.HasValue || d.NgayKetThucDot.Value >= today))
                .FirstOrDefaultAsync();

            if (dot == null) return RedirectToAction("Index");

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                    .ThenInclude(dt => dt!.IdGvhdNavigation)
                        .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            // Lấy danh sách sinh viên cùng đề tài (nhóm)
            var dsSinhVienCungDeTai = await GetDanhSachIdSinhVienCungDeTai(sinhVien.IdNguoiDung, dot.Id);
            var danhSachNhom = await GetDanhSachSinhVienNhom(sinhVien.IdNguoiDung, dot.Id);

            // Tìm báo cáo của nhóm (bất kỳ ai trong nhóm nộp)
            var baoCao = await _context.BaoCaoNops
                .Include(b => b.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(b => dsSinhVienCungDeTai.Contains(b.IdSinhVien ?? 0) &&
                                          b.IdDot == dot.Id &&
                                          b.LoaiBaoCao == loai);

            DateOnly? batDau = null, ketThuc = null;
            string tieuDe = "";
            string yeuCau = "";

            switch (loai)
            {
                case "DE_CUONG":
                    tieuDe = "Nộp đề cương";
                    batDau = dot.NgayBatDauNopDeCuong;
                    ketThuc = dot.NgayKetThucNopDeCuong;
                    yeuCau = "File PDF đề cương đồ án (bao gồm: Mục tiêu, Phạm vi, Phương pháp nghiên cứu, Kế hoạch thực hiện)";
                    break;
                case "GIUA_KY":
                    tieuDe = "Nộp báo cáo giữa kỳ";
                    batDau = dot.NgayBatDauBaoCaoGiuaKi;
                    ketThuc = dot.NgayKetThucBaoCaoGiuaKi;
                    yeuCau = "File PDF báo cáo tiến độ giữa kỳ (bao gồm: Tiến độ thực hiện, Khó khăn gặp phải, Kế hoạch tiếp theo)";
                    break;
                case "CUOI_KY":
                    tieuDe = "Nộp báo cáo cuối kỳ";
                    batDau = dot.NgayBatDauBaoCaoCuoiKi;
                    ketThuc = dot.NgayKetThucBaoCaoCuoiKi;
                    yeuCau = "File PDF báo cáo đồ án hoàn chỉnh (theo mẫu quy định của Khoa)";
                    break;
            }

            // Xác định trạng thái giai đoạn
            var (trangThaiGiaiDoan, trangThaiGiaiDoanText, trangThaiGiaiDoanCss) = GetTrangThaiGiaiDoan(batDau, ketThuc, today);
            var dangMo = trangThaiGiaiDoan == "DANG_MO";

            var trangThai = baoCao?.TrangThai ?? "CHUA_NOP";
            var hasFile = !string.IsNullOrEmpty(baoCao?.FileBaocao);

            // Tính số ngày còn lại
            int? soNgayConLai = null;
            bool sapHetHan = false;
            if (ketThuc.HasValue && dangMo)
            {
                soNgayConLai = ketThuc.Value.DayNumber - today.DayNumber;
                sapHetHan = soNgayConLai <= 2;
            }

            // Xác định lý do không thể nộp
            string? lyDoKhongTheNop = null;
            var canUpload = dangMo && trangThai != "DA_DUYET";
            var canDelete = dangMo && hasFile && trangThai != "DA_DUYET";

            if (!canUpload)
            {
                if (trangThai == "DA_DUYET")
                    lyDoKhongTheNop = "Báo cáo đã được duyệt, không thể chỉnh sửa.";
                else if (trangThaiGiaiDoan == "CHUA_MO")
                    lyDoKhongTheNop = $"Chưa đến thời gian nộp. Thời gian nộp: {batDau?.ToString("dd/MM/yyyy")} - {ketThuc?.ToString("dd/MM/yyyy")}";
                else if (trangThaiGiaiDoan == "DA_DONG")
                    lyDoKhongTheNop = "Đã hết thời gian nộp báo cáo. Vui lòng liên hệ GVHD nếu cần hỗ trợ.";
                else if (trangThaiGiaiDoan == "CHUA_CAU_HINH")
                    lyDoKhongTheNop = "Chưa cấu hình thời gian nộp. Vui lòng liên hệ Ban chủ nhiệm khoa.";
            }

            // Kiểm tra người nộp
            string? nguoiNop = null;
            bool laNguoiDaiDien = false;
            if (baoCao?.IdSinhVienNavigation != null)
            {
                var tenNguoiNop = baoCao.IdSinhVienNavigation.IdNguoiDungNavigation?.HoTen;
                var mssvNguoiNop = baoCao.IdSinhVienNavigation.Mssv;
                nguoiNop = $"{mssvNguoiNop} - {tenNguoiNop}";
                laNguoiDaiDien = baoCao.IdSinhVien == sinhVien.IdNguoiDung;
            }

            // Lấy tên GVHD
            var tenGVHD = svDeTai?.IdDeTaiNavigation?.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen;

            // Tính kích thước file
            string? kichThuocFile = null;
            if (hasFile && baoCao?.FileBaocao != null)
            {
                var physicalPath = Path.Combine(_env.WebRootPath, baoCao.FileBaocao.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    var fileInfo = new FileInfo(physicalPath);
                    kichThuocFile = FormatFileSize(fileInfo.Length);
                }
            }

            var vm = new NopBaoCaoDetailViewModel
            {
                IdBaoCaoNop = baoCao?.Id,
                LoaiBaoCao = loai,
                TieuDe = tieuDe,
                TenDot = dot.TenDot,
                MaDeTai = svDeTai?.IdDeTaiNavigation?.MaDeTai,
                TenDeTai = svDeTai?.IdDeTaiNavigation?.TenDeTai,
                TenGVHD = tenGVHD,
                TrangThaiGui = hasFile ? "Đã nộp" : "Chưa nộp",
                TrangThai = trangThai,
                TrangThaiText = GetTrangThaiText(trangThai),
                TrangThaiCss = GetTrangThaiCss(trangThai),
                TrangThaiGiaiDoan = trangThaiGiaiDoan,
                TrangThaiGiaiDoanText = trangThaiGiaiDoanText,
                TrangThaiGiaiDoanCss = trangThaiGiaiDoanCss,
                TenFile = GetFileName(baoCao?.FileBaocao),
                FilePath = baoCao?.FileBaocao,
                NgayNop = baoCao?.NgayNop?.ToString("dd/MM/yyyy HH:mm"),
                NgaySuaDoiCuoi = baoCao?.NgaySuaDoiCuoi?.ToString("dd/MM/yyyy HH:mm"),
                KichThuocFile = kichThuocFile,
                NguoiNop = nguoiNop,
                LaNguoiDaiDien = laNguoiDaiDien,
                GhiChuGui = baoCao?.GhiChuGui,
                NhanXetGVHD = baoCao?.NhanXet,
                ThoiGianBatDau = batDau?.ToString("dd/MM/yyyy"),
                ThoiGianKetThuc = ketThuc?.ToString("dd/MM/yyyy"),
                SoNgayConLai = soNgayConLai,
                SapHetHan = sapHetHan,
                DangMo = dangMo,
                CanUpload = canUpload,
                CanDelete = canDelete,
                LyDoKhongTheNop = lyDoKhongTheNop,
                YeuCauBaoCao = yeuCau,
                IdDot = dot.Id,
                IdDeTai = svDeTai?.IdDeTai,
                IdSinhVien = sinhVien.IdNguoiDung,
                DanhSachNhom = danhSachNhom
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopFile(string loai, IFormFile? file, string? ghiChu)
        {
            if (string.IsNullOrEmpty(loai))
                return RedirectToAction("Index");

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);
            if (sinhVien == null) return RedirectToAction("Index");

            // Tìm đợt đang mở (chưa kết thúc)
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true && 
                           (!d.NgayKetThucDot.HasValue || d.NgayKetThucDot.Value >= today))
                .FirstOrDefaultAsync();
            if (dot == null) 
            {
                TempData["Error"] = "Không tìm thấy đợt đồ án đang mở.";
                return RedirectToAction("Index");
            }

            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt =>
                    svdt.IdSinhVien == sinhVien.IdNguoiDung &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == dot.Id &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            // RÀNG BUỘC: Kiểm tra giai đoạn nộp theo từng loại báo cáo
            DateOnly? batDau = null, ketThuc = null;
            string tenLoaiBaoCao = "";

            switch (loai)
            {
                case "DE_CUONG":
                    batDau = dot.NgayBatDauNopDeCuong;
                    ketThuc = dot.NgayKetThucNopDeCuong;
                    tenLoaiBaoCao = "đề cương";
                    break;
                case "GIUA_KY":
                    batDau = dot.NgayBatDauBaoCaoGiuaKi;
                    ketThuc = dot.NgayKetThucBaoCaoGiuaKi;
                    tenLoaiBaoCao = "báo cáo giữa kỳ";
                    break;
                case "CUOI_KY":
                    batDau = dot.NgayBatDauBaoCaoCuoiKi;
                    ketThuc = dot.NgayKetThucBaoCaoCuoiKi;
                    tenLoaiBaoCao = "báo cáo cuối kỳ";
                    break;
            }

            // Kiểm tra cấu hình thời gian
            if (!batDau.HasValue || !ketThuc.HasValue)
            {
                TempData["Error"] = $"Chưa cấu hình thời gian nộp {tenLoaiBaoCao}. Vui lòng liên hệ Ban chủ nhiệm khoa.";
                return RedirectToAction("Detail", new { loai });
            }

            // Kiểm tra chưa đến thời gian
            if (today < batDau.Value)
            {
                TempData["Error"] = $"Chưa đến thời gian nộp {tenLoaiBaoCao}. Thời gian nộp bắt đầu từ {batDau.Value:dd/MM/yyyy}.";
                return RedirectToAction("Detail", new { loai });
            }

            // Kiểm tra đã hết thời gian
            if (today > ketThuc.Value)
            {
                TempData["Error"] = $"Đã hết thời gian nộp {tenLoaiBaoCao}. Hạn cuối là {ketThuc.Value:dd/MM/yyyy}. Vui lòng liên hệ GVHD.";
                return RedirectToAction("Detail", new { loai });
            }

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file để nộp.";
                return RedirectToAction("Detail", new { loai });
            }

            // RÀNG BUỘC 2: Chỉ chấp nhận file PDF
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".pdf")
            {
                TempData["Error"] = "Chỉ chấp nhận file PDF. Vui lòng chọn file PDF.";
                return RedirectToAction("Detail", new { loai });
            }

            // Lấy danh sách sinh viên cùng đề tài
            var dsSinhVienCungDeTai = await GetDanhSachIdSinhVienCungDeTai(sinhVien.IdNguoiDung, dot.Id);

            // Kiểm tra báo cáo hiện tại của nhóm
            var baoCaoHienTai = await _context.BaoCaoNops
                .FirstOrDefaultAsync(b => dsSinhVienCungDeTai.Contains(b.IdSinhVien ?? 0) &&
                                          b.IdDot == dot.Id &&
                                          b.LoaiBaoCao == loai);

            // Nếu đã có báo cáo và đã duyệt, không cho nộp lại
            if (baoCaoHienTai?.TrangThai == "DA_DUYET")
            {
                TempData["Error"] = "Báo cáo đã được duyệt, không thể nộp lại.";
                return RedirectToAction("Detail", new { loai });
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "baocao");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var uniqueName = $"{mssv}_{loai}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsDir, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/uploads/baocao/{uniqueName}";
            var now = DateTime.Now;

            if (baoCaoHienTai == null)
            {
                // Tạo mới báo cáo cho sinh viên nộp (đại diện)
                baoCaoHienTai = new BaoCaoNop
                {
                    IdDot = dot.Id,
                    IdDeTai = svDeTai?.IdDeTai,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    Stt = loai == "DE_CUONG" ? 1 : loai == "GIUA_KY" ? 2 : 3,
                    TenBaoCao = loai switch
                    {
                        "DE_CUONG" => "Đề cương đồ án",
                        "GIUA_KY" => "Báo cáo giữa kỳ",
                        "CUOI_KY" => "Báo cáo cuối kỳ",
                        _ => "Báo cáo"
                    },
                    FileBaocao = relativePath,
                    NgayNop = now,
                    TrangThai = "CHO_DUYET",
                    LoaiBaoCao = loai,
                    GhiChuGui = ghiChu,
                    NgaySuaDoiCuoi = now
                };
                _context.BaoCaoNops.Add(baoCaoHienTai);
            }
            else
            {
                // Cập nhật báo cáo hiện tại
                baoCaoHienTai.FileBaocao = relativePath;
                baoCaoHienTai.IdSinhVien = sinhVien.IdNguoiDung; // Cập nhật người nộp
                baoCaoHienTai.NgayNop = now;
                baoCaoHienTai.TrangThai = "CHO_DUYET";
                baoCaoHienTai.GhiChuGui = ghiChu;
                baoCaoHienTai.NgaySuaDoiCuoi = now;
                baoCaoHienTai.NhanXet = null;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Nộp file PDF thành công!";
            return RedirectToAction("Detail", new { loai });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaFile(string loai)
        {
            if (string.IsNullOrEmpty(loai))
                return Json(new { success = false, message = "Loại báo cáo không hợp lệ." });

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Mssv == mssv);
            if (sinhVien == null)
                return Json(new { success = false, message = "Không tìm thấy sinh viên." });

            // Tìm đợt đang mở (chưa kết thúc)
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dot = await _context.DotDoAns
                .Where(d => d.TrangThai == true && 
                           (!d.NgayKetThucDot.HasValue || d.NgayKetThucDot.Value >= today))
                .FirstOrDefaultAsync();
            if (dot == null)
                return Json(new { success = false, message = "Không tìm thấy đợt đồ án đang mở." });

            // Kiểm tra giai đoạn nộp theo từng loại
            DateOnly? batDau = null, ketThuc = null;
            string tenLoaiBaoCao = "";

            switch (loai)
            {
                case "DE_CUONG":
                    batDau = dot.NgayBatDauNopDeCuong;
                    ketThuc = dot.NgayKetThucNopDeCuong;
                    tenLoaiBaoCao = "đề cương";
                    break;
                case "GIUA_KY":
                    batDau = dot.NgayBatDauBaoCaoGiuaKi;
                    ketThuc = dot.NgayKetThucBaoCaoGiuaKi;
                    tenLoaiBaoCao = "báo cáo giữa kỳ";
                    break;
                case "CUOI_KY":
                    batDau = dot.NgayBatDauBaoCaoCuoiKi;
                    ketThuc = dot.NgayKetThucBaoCaoCuoiKi;
                    tenLoaiBaoCao = "báo cáo cuối kỳ";
                    break;
            }

            // Kiểm tra cấu hình thời gian
            if (!batDau.HasValue || !ketThuc.HasValue)
                return Json(new { success = false, message = $"Chưa cấu hình thời gian nộp {tenLoaiBaoCao}." });

            // Kiểm tra trong khoảng thời gian nộp
            if (today < batDau.Value)
                return Json(new { success = false, message = $"Chưa đến thời gian nộp {tenLoaiBaoCao}." });

            if (today > ketThuc.Value)
                return Json(new { success = false, message = $"Đã hết thời gian nộp {tenLoaiBaoCao}." });

            var dsSinhVienCungDeTai = await GetDanhSachIdSinhVienCungDeTai(sinhVien.IdNguoiDung, dot.Id);


            var baoCao = await _context.BaoCaoNops
                .FirstOrDefaultAsync(b => dsSinhVienCungDeTai.Contains(b.IdSinhVien ?? 0) &&
                                          b.IdDot == dot.Id &&
                                          b.LoaiBaoCao == loai);

            if (baoCao == null)
                return Json(new { success = false, message = "Không tìm thấy báo cáo." });

            if (baoCao.TrangThai == "DA_DUYET")
                return Json(new { success = false, message = "Báo cáo đã được duyệt, không thể xóa." });

            // Xóa file vật lý
            if (!string.IsNullOrEmpty(baoCao.FileBaocao))
            {
                var physicalPath = Path.Combine(_env.WebRootPath, baoCao.FileBaocao.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            // Reset thông tin file
            baoCao.FileBaocao = null;
            baoCao.TrangThai = "CHUA_NOP";
            baoCao.NgaySuaDoiCuoi = DateTime.Now;
            baoCao.NhanXet = null;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa file thành công!" });
        }

        // ============ PRIVATE METHODS ============

        private async Task<List<int>> GetDanhSachIdSinhVienCungDeTai(int idSinhVien, int idDot)
        {
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == idSinhVien &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == idDot &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            if (svDeTai == null) return new List<int> { idSinhVien };

            return await _context.SinhVienDeTais
                .Where(svdt => svdt.IdDeTai == svDeTai.IdDeTai &&
                    svdt.IdSinhVien != null &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"))
                .Select(svdt => svdt.IdSinhVien!.Value)
                .ToListAsync();
        }

        private async Task<List<SinhVienNhomItem>> GetDanhSachSinhVienNhom(int idSinhVien, int idDot)
        {
            var svDeTai = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdDeTaiNavigation)
                .FirstOrDefaultAsync(svdt => svdt.IdSinhVien == idSinhVien &&
                    svdt.IdDeTaiNavigation != null &&
                    svdt.IdDeTaiNavigation.IdDot == idDot &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"));

            if (svDeTai == null) return new();

            return await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(svdt => svdt.IdDeTai == svDeTai.IdDeTai &&
                    (svdt.TrangThai == "DA_DUYET" || svdt.TrangThai == "Đã duyệt"))
                .Select(svdt => new SinhVienNhomItem
                {
                    IdSinhVien = svdt.IdSinhVien ?? 0,
                    Mssv = svdt.IdSinhVienNavigation!.Mssv,
                    HoTen = svdt.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen
                })
                .ToListAsync();
        }

        private NopBaoCaoBoxItem BuildBoxItem(List<BaoCaoNop> baoCaos, string loai, string tieuDe,
            DateOnly? batDau, DateOnly? ketThuc, DateOnly today)
        {
            var bc = baoCaos.FirstOrDefault(b => b.LoaiBaoCao == loai);
            var trangThai = bc?.TrangThai ?? "CHUA_NOP";
            var dangMo = batDau.HasValue && ketThuc.HasValue && today >= batDau && today <= ketThuc;

            // Xác định trạng thái giai đoạn: CHUA_MO, DANG_MO, DA_DONG
            var (trangThaiGiaiDoan, trangThaiGiaiDoanText, trangThaiGiaiDoanCss) = GetTrangThaiGiaiDoan(batDau, ketThuc, today);

            return new NopBaoCaoBoxItem
            {
                IdBaoCaoNop = bc?.Id,
                LoaiBaoCao = loai,
                TieuDe = tieuDe,
                ThoiGianBatDau = batDau?.ToString("dd/MM/yyyy"),
                ThoiGianKetThuc = ketThuc?.ToString("dd/MM/yyyy"),
                TrangThai = trangThai,
                TrangThaiText = GetTrangThaiText(trangThai),
                TrangThaiCss = GetTrangThaiCss(trangThai),
                TrangThaiGiaiDoan = trangThaiGiaiDoan,
                TrangThaiGiaiDoanText = trangThaiGiaiDoanText,
                TrangThaiGiaiDoanCss = trangThaiGiaiDoanCss,
                DangMo = dangMo
            };
        }

        /// <summary>
        /// Xác định trạng thái giai đoạn: Chưa mở, Đang mở, Đã đóng
        /// </summary>
        private static (string Code, string Text, string Css) GetTrangThaiGiaiDoan(DateOnly? batDau, DateOnly? ketThuc, DateOnly today)
        {
            // Nếu chưa có cấu hình thời gian
            if (!batDau.HasValue || !ketThuc.HasValue)
            {
                return ("CHUA_CAU_HINH", "Chưa cấu hình", "phase-unconfigured");
            }

            // Chưa đến thời gian bắt đầu
            if (today < batDau.Value)
            {
                return ("CHUA_MO", "Chưa mở", "phase-pending");
            }

            // Đang trong thời gian mở
            if (today >= batDau.Value && today <= ketThuc.Value)
            {
                return ("DANG_MO", "Đang mở", "phase-open");
            }

            // Đã qua thời gian kết thúc
            return ("DA_DONG", "Đã đóng", "phase-closed");
        }

        private static string GetTrangThaiText(string? trangThai) => trangThai switch
        {
            "DA_DUYET" => "Đã duyệt",
            "CHO_DUYET" => "Chờ duyệt",
            "TU_CHOI" => "Từ chối",
            "CHUA_NOP" => "Chưa nộp",
            _ => "Chưa nộp"
        };

        private static string GetTrangThaiCss(string? trangThai) => trangThai switch
        {
            "DA_DUYET" => "status-approved",
            "CHO_DUYET" => "status-pending",
            "TU_CHOI" => "status-rejected",
            _ => "status-default"
        };

        private static string? GetFileName(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return null;
            return filePath.Contains('/') ? filePath[(filePath.LastIndexOf('/') + 1)..] : filePath;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
