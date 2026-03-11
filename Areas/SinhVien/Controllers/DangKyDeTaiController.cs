using DATN_TMS.Areas.SinhVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.SinhVien.Controllers
{
    /// <summary>
    /// Controller đăng ký đề tài cho sinh viên
    /// Kế thừa BaseSinhVienController để kiểm tra nguyện vọng đã duyệt
    /// </summary>
    public class DangKyDeTaiController : BaseSinhVienController
    {
        private const int MAX_SV_PER_DETAI = 2;

        public DangKyDeTaiController(QuanLyDoAnTotNghiepContext context) : base(context)
        {
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

            var giaiDoan = XacDinhGiaiDoanDangKy(dot);
            ViewBag.GiaiDoan = giaiDoan;
            ViewBag.TenDot = dot.TenDot;

            if (giaiDoan == "CHUA_MO")
            {
                var ngayMo = dot.NgayBatDauDkDeTai?.ToString("dd/MM/yyyy");
                ViewBag.Message = $"Giai đoạn đăng ký đề tài sẽ bắt đầu từ ngày {ngayMo}. Vui lòng quay lại sau.";
                return View(new List<DangKyDeTaiViewModel>().ToPagedList(1, pageSize));
            }

            var svId = sinhVien.IdNguoiDung;

            // ============================================
            // Lấy tất cả đăng ký của SV trong đợt này (kèm trạng thái)
            // ============================================
            var dangKyCuaSV = await _context.SinhVienDeTais
                .Where(x => x.IdSinhVien == svId && x.IdDeTaiNavigation!.IdDot == dot.Id)
                .Select(x => new { x.IdDeTai, x.TrangThai })
                .ToListAsync();

            // Kiểm tra SV đã được duyệt vào đề tài nào chưa
            var deTaiDaDuyet = dangKyCuaSV
                .FirstOrDefault(x => x.TrangThai == "Đã duyệt" || x.TrangThai == "DA_DUYET");
            var svDaDuyetDeTaiKhac = deTaiDaDuyet != null;
            var idDeTaiDaDuyet = deTaiDaDuyet?.IdDeTai;

            // Danh sách ID đề tài đã đăng ký (bất kể trạng thái)
            var daDangKyIdDeTai = dangKyCuaSV.Select(x => x.IdDeTai).ToList();

            // ============================================
            // BUSINESS RULE #3: Lấy danh sách ID của SinhVien để đánh dấu đề tài tự đề xuất
            // ============================================
            var danhSachIdSinhVien = await _context.SinhViens.Select(sv => sv.IdNguoiDung).ToListAsync();

            // Sinh viên có thể đăng ký vào các đề tài đã đề xuất (CHO_DUYET hoặc DA_DUYET)
            var query = _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv!.IdNguoiDungNavigation)
                .Where(dt => dt.IdDot == dot.Id && (dt.TrangThai == "CHO_DUYET" || dt.TrangThai == "DA_DUYET"))
                .AsQueryable();

            // Build filter dropdown data from full (unfiltered) list
            var allChuyenNganhs = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Where(dt => dt.IdDot == dot.Id && (dt.TrangThai == "CHO_DUYET" || dt.TrangThai == "DA_DUYET") && dt.IdChuyenNganhNavigation != null)
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

            // Lấy số SV đã duyệt cho mỗi đề tài
            var deTaiIds = await query.Select(dt => dt.Id).ToListAsync();
            var soSvDaDuyetDict = await _context.SinhVienDeTais
                .Where(svdt => deTaiIds.Contains(svdt.IdDeTai ?? 0) 
                    && (svdt.TrangThai == "Đã duyệt" || svdt.TrangThai == "DA_DUYET"))
                .GroupBy(svdt => svdt.IdDeTai)
                .Select(g => new { IdDeTai = g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.IdDeTai ?? 0, x => x.SoLuong);

            // Lấy danh sách đề tài với trạng thái đăng ký
            var deTaiList = await query.OrderByDescending(dt => dt.Id).ToListAsync();

            var data = deTaiList.Select(dt => {
                // Xác định trạng thái đăng ký của SV với đề tài này
                var dangKy = dangKyCuaSV.FirstOrDefault(x => x.IdDeTai == dt.Id);
                var svDaDuyetDeTaiNay = dangKy != null && (dangKy.TrangThai == "Đã duyệt" || dangKy.TrangThai == "DA_DUYET");
                var svDaDangKyDeTaiNay = dangKy != null && !svDaDuyetDeTaiNay;

                // Xác định text hiển thị cột Trạng thái
                string? trangThaiDangKyCuaSV = null;
                if (svDaDuyetDeTaiNay)
                {
                    trangThaiDangKyCuaSV = "Đã duyệt";
                }
                else if (svDaDangKyDeTaiNay)
                {
                    trangThaiDangKyCuaSV = "Đã đăng ký";
                }
                // Nếu SV đã được duyệt ở đề tài khác → để trống

                return new DangKyDeTaiViewModel
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    Nganh = dt.IdChuyenNganhNavigation?.TenChuyenNganh ?? "",
                    GVHD = dt.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen ?? "",
                    TrangThai = dt.TrangThai,
                    StatusCss = dt.TrangThai == "DA_DUYET" ? "badge-green" :
                                dt.TrangThai == "CHO_DUYET" ? "badge-orange" :
                                dt.TrangThai == "TU_CHOI" ? "badge-red" : "badge-dark",
                    DaDangKy = daDangKyIdDeTai.Contains(dt.Id),
                    // BUSINESS RULE #3: Đánh dấu nếu người đề xuất là sinh viên
                    LaDeTaiSVTuDeXuat = dt.IdNguoiDeXuat.HasValue && danhSachIdSinhVien.Contains(dt.IdNguoiDeXuat.Value),
                    // Trạng thái sĩ số
                    SoSinhVienDaDuyet = soSvDaDuyetDict.ContainsKey(dt.Id) ? soSvDaDuyetDict[dt.Id] : 0,
                    SoSlotToiDa = MAX_SV_PER_DETAI,
                    // Trạng thái đăng ký của SV
                    TrangThaiDangKyCuaSV = trangThaiDangKyCuaSV,
                    SvDaDuyetDeTaiNay = svDaDuyetDeTaiNay,
                    SvDaDangKyDeTaiNay = svDaDangKyDeTaiNay
                };
            });

            var pagedList = data.ToPagedList(pageNumber, pageSize);

            // Truyền thêm thông tin SV đã được duyệt đề tài khác
            ViewBag.SvDaDuyetDeTaiKhac = svDaDuyetDeTaiKhac;

            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dot = await GetDotDoAnActive();
            if (dot == null) return RedirectToAction("Index");

            var giaiDoan = XacDinhGiaiDoanDangKy(dot);
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
                .OrderBy(svdt => svdt.NgayDangKy)
                .ToListAsync();

            var svId = sinhVien.IdNguoiDung;

            // ============================================
            // LOGIC: Nhóm các SV theo thời điểm đăng ký (cùng giây = cùng nhóm)
            // ============================================
            var danhSachLuotDangKy = new List<LuotDangKyItem>();
            var daXuLy = new HashSet<int>();
            var coNhomDaDuyet = false;

            foreach (var sv1 in allDangKy)
            {
                if (daXuLy.Contains(sv1.Id)) continue;
                daXuLy.Add(sv1.Id);

                // Tìm SV cùng nhóm (đăng ký cùng thời điểm, chênh lệch < 5 giây)
                SinhVienDeTai? sv2 = null;
                foreach (var svCandidate in allDangKy)
                {
                    if (daXuLy.Contains(svCandidate.Id)) continue;
                    if (sv1.NgayDangKy.HasValue && svCandidate.NgayDangKy.HasValue)
                    {
                        var diff = Math.Abs((sv1.NgayDangKy.Value - svCandidate.NgayDangKy.Value).TotalSeconds);
                        if (diff < 5)
                        {
                            sv2 = svCandidate;
                            daXuLy.Add(svCandidate.Id);
                            break;
                        }
                    }
                }

                var laSvHienTai = sv1.IdSinhVien == svId || (sv2?.IdSinhVien == svId);
                var daDuyet = sv1.TrangThai == "Đã duyệt" || sv1.TrangThai == "DA_DUYET";

                if (daDuyet) coNhomDaDuyet = true;

                danhSachLuotDangKy.Add(new LuotDangKyItem
                {
                    IdLuotDangKy = sv1.Id,
                    TrangThai = sv1.TrangThai,
                    StatusCss = sv1.TrangThai switch
                    {
                        "Đã duyệt" or "DA_DUYET" => "member-approved",
                        "Chờ GVHD duyệt" => "member-pending",
                        "Từ chối" => "member-rejected",
                        _ => "member-default"
                                },
                                NgayDangKy = sv1.NgayDangKy?.ToString("dd/MM/yyyy HH:mm"),
                                LaSvHienTai = laSvHienTai,
                                DaDuyet = daDuyet,

                                IdSinhVien1 = sv1.IdSinhVien ?? 0,
                                Mssv1 = sv1.IdSinhVienNavigation?.Mssv,
                                HoTen1 = sv1.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,

                                IdSinhVien2 = sv2?.IdSinhVien,
                                Mssv2 = sv2?.IdSinhVienNavigation?.Mssv,
                                HoTen2 = sv2?.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen
                            });
            }

            // ============================================
            // LOGIC LỌC: Ẩn thông tin đăng ký chưa duyệt của SV khác
            // - Chỉ hiển thị: Đã duyệt HOẶC là nhóm của chính SV hiện tại
            // - SV B không thể thấy SV A đang chờ duyệt
            // ============================================
            var danhSachHienThi = danhSachLuotDangKy
                .Where(l => l.DaDuyet || l.LaSvHienTai)
                .ToList();

            var svDangKyTrongDot = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == svId
                    && x.IdDeTai != id
                    && x.IdDeTaiNavigation!.IdDot == dot.Id);

                                var svDangKyDeTaiNay = allDangKy.Any(x => x.IdSinhVien == svId);
                                var trangThaiSV = allDangKy.FirstOrDefault(x => x.IdSinhVien == svId)?.TrangThai;

                                // Số lượt đăng ký = số nhóm đã xử lý (tất cả, không chỉ hiển thị)
                                var soLuotDangKy = danhSachLuotDangKy.Count;
                                var soLuotDaDuyet = danhSachLuotDangKy.Count(x => x.DaDuyet);

                                // Tính số SV đã duyệt chính thức
                                var soSvDaDuyet = await _context.SinhVienDeTais
                                    .CountAsync(svdt => svdt.IdDeTai == id 
                                        && (svdt.TrangThai == "Đã duyệt" || svdt.TrangThai == "DA_DUYET"));

                                // ============================================
                                // Kiểm tra SV đã được duyệt vào đề tài KHÁC chưa
                                // ============================================
                                var svDaDuyetDeTaiKhac = await _context.SinhVienDeTais
                                    .AnyAsync(x => x.IdSinhVien == svId
                                        && x.IdDeTai != id
                                        && x.IdDeTaiNavigation!.IdDot == dot.Id
                                        && (x.TrangThai == "Đã duyệt" || x.TrangThai == "DA_DUYET"));

                                // ============================================
                                // BUSINESS RULE #3: Kiểm tra đề tài do SV tự đề xuất
                                // ============================================
                                var laDeTaiSVTuDeXuat = await _context.SinhViens
                                    .AnyAsync(sv => sv.IdNguoiDung == deTai.IdNguoiDeXuat);

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
                                    NhiemVuCuThe = deTai.NhiemVuCuThe,
                                    TrangThaiDeTai = deTai.TrangThai,

                                    MssvSinhVien = sinhVien.Mssv,
                                    HoTenSinhVien = sinhVien.IdNguoiDungNavigation?.HoTen,
                                    IdSinhVienHienTai = sinhVien.IdNguoiDung,
                                    DaDangKyDeTaiNay = svDangKyDeTaiNay,
                                    DaDangKyDeTaiKhac = false, // Không còn chặn đăng ký nhiều đề tài
                                    SvDaDuyetDeTaiKhac = svDaDuyetDeTaiKhac, // Chặn nếu đã được duyệt ở đề tài khác
                                    TrangThaiDangKyCuaSV = trangThaiSV,
                                    GiaiDoan = giaiDoan,

                                    // Sử dụng danh sách đã lọc
                                    DanhSachLuotDangKy = danhSachHienThi,
                                    SoLuotDangKy = soLuotDangKy,
                                    SoLuotDaDuyet = soLuotDaDuyet,
                                    DaHetLuotDangKy = soSvDaDuyet >= MAX_SV_PER_DETAI, // Đã đủ số SV được duyệt

                                    // Trạng thái sĩ số
                                    SoSinhVienDaDuyet = soSvDaDuyet,
                                    SoSlotToiDa = MAX_SV_PER_DETAI,

                                    // BUSINESS RULE #3
                                    LaDeTaiSVTuDeXuat = laDeTaiSVTuDeXuat,

                                    // ID đợt để tìm kiếm SV thứ 2
                                    IdDot = dot.Id
                                };

                                return View(vm);
                            }

        // POST: Đăng ký đề tài (có thể kèm SV thứ 2)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(int idDeTai, int? idSinhVien2 = null)
        {
            var dot = await GetDotDoAnActive();
            if (dot == null)
                return Json(new { success = false, message = "Không tìm thấy đợt đồ án." });

            var giaiDoan = XacDinhGiaiDoanDangKy(dot);
            if (giaiDoan != "DANG_MO")
                return Json(new { success = false, message = "Giai đoạn đăng ký đã đóng." });

            var mssv = HttpContext.Session.GetString("UserCode");
            var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(x => x.Mssv == mssv);
            if (sinhVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên." });

            var deTai = await _context.DeTais
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == idDeTai && dt.IdDot == dot.Id);
            if (deTai == null)
                return Json(new { success = false, message = "Đề tài không tồn tại hoặc không thuộc đợt hiện tại." });

            // ============================================
            // BUSINESS RULE #3: Khóa đề tài tự đề xuất
            // Kiểm tra nếu đề tài do SV tự đề xuất => chặn đăng ký
            // ============================================
            var nguoiDeXuatLaSinhVien = await _context.SinhViens
                .AnyAsync(sv => sv.IdNguoiDung == deTai.IdNguoiDeXuat);

            if (nguoiDeXuatLaSinhVien)
            {
                return Json(new { success = false, message = "Đề tài này do sinh viên tự đề xuất, không thể đăng ký." });
            }

            // ============================================
            // NEW: Kiểm tra SV đã được duyệt vào đề tài nào chưa
            // Nếu đã được duyệt → chặn toàn bộ quyền đăng ký thêm
            // ============================================
            var svDaDuyetDeTaiKhac = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung
                    && x.IdDeTaiNavigation!.IdDot == dot.Id
                    && (x.TrangThai == "Đã duyệt" || x.TrangThai == "DA_DUYET"));

            if (svDaDuyetDeTaiKhac)
                return Json(new { success = false, message = "Bạn đã được duyệt vào một đề tài, không thể đăng ký thêm." });

            // Kiểm tra đã đăng ký đề tài này chưa
            var daDangKyDeTaiNay = await _context.SinhVienDeTais
                .AnyAsync(x => x.IdSinhVien == sinhVien.IdNguoiDung && x.IdDeTai == idDeTai);
            if (daDangKyDeTaiNay)
                return Json(new { success = false, message = "Bạn đã đăng ký đề tài này rồi." });

            // ============================================
            // Kiểm tra đề tài đã đủ số lượng SV được duyệt chưa
            // ============================================
            var soSvDaDuyet = await _context.SinhVienDeTais
                .CountAsync(x => x.IdDeTai == idDeTai 
                    && (x.TrangThai == "Đã duyệt" || x.TrangThai == "DA_DUYET"));
            if (soSvDaDuyet >= MAX_SV_PER_DETAI)
                return Json(new { success = false, message = $"Đề tài đã đủ số lượng sinh viên (tối đa {MAX_SV_PER_DETAI})." });

            // ============================================
            // Kiểm tra sinh viên thứ 2 (nếu có)
            // ============================================
            DATN_TMS.Models.SinhVien? sinhVien2 = null;
            if (idSinhVien2.HasValue && idSinhVien2.Value > 0)
            {
                sinhVien2 = await _context.SinhViens
                    .Include(sv => sv.IdNguoiDungNavigation)
                    .FirstOrDefaultAsync(sv => sv.IdNguoiDung == idSinhVien2.Value);

                if (sinhVien2 == null)
                    return Json(new { success = false, message = "Không tìm thấy thông tin sinh viên thứ 2." });

                // Kiểm tra SV2 đã được duyệt nguyện vọng chưa
                var sv2DaDuyetNguyenVong = await _context.DangKyNguyenVongs
                    .AnyAsync(dk => dk.IdSinhVien == idSinhVien2.Value && dk.IdDot == dot.Id && dk.TrangThai == 1);
                if (!sv2DaDuyetNguyenVong)
                    return Json(new { success = false, message = "Sinh viên thứ 2 chưa được duyệt nguyện vọng trong đợt này." });

                // Kiểm tra SV2 đã có đề tài chưa
                var sv2DaCoDeTai = await _context.SinhVienDeTais
                    .AnyAsync(svdt => svdt.IdSinhVien == idSinhVien2.Value && svdt.IdDeTaiNavigation!.IdDot == dot.Id);
                if (sv2DaCoDeTai)
                    return Json(new { success = false, message = $"Sinh viên {sinhVien2.Mssv} đã có đề tài, không thể thêm vào nhóm." });
            }

            try
            {
                // Thêm sinh viên đại diện (người đăng ký)
                var svdt1 = new SinhVienDeTai
                {
                    IdDeTai = deTai.Id,
                    IdSinhVien = sinhVien.IdNguoiDung,
                    TrangThai = "Chờ GVHD duyệt",
                    NgayDangKy = DateTime.Now
                };
                _context.SinhVienDeTais.Add(svdt1);

                // Thêm sinh viên thứ 2 (nếu có)
                if (sinhVien2 != null)
                {
                    var svdt2 = new SinhVienDeTai
                    {
                        IdDeTai = deTai.Id,
                        IdSinhVien = sinhVien2.IdNguoiDung,
                        TrangThai = "Chờ GVHD duyệt",
                        NgayDangKy = DateTime.Now
                    };
                    _context.SinhVienDeTais.Add(svdt2);
                }

                await _context.SaveChangesAsync();

                var message = sinhVien2 != null
                    ? $"Đăng ký đề tài thành công cùng với {sinhVien2.IdNguoiDungNavigation?.HoTen}! Vui lòng chờ giảng viên duyệt."
                    : "Đăng ký đề tài thành công! Vui lòng chờ giảng viên duyệt.";

                return Json(new { success = true, message = message });
            }
            catch
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng thử lại sau." });
            }
        }

        // API tìm kiếm sinh viên thứ 2 (chỉ SV đã được duyệt nguyện vọng trong đợt hiện tại)
        [HttpGet]
        public async Task<IActionResult> TimKiemSinhVien2(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return Json(new { success = true, data = new List<object>() });
            }

            var dot = await GetDotDoAnActive();
            if (dot == null)
            {
                return Json(new { success = false, message = "Không có đợt đang mở." });
            }

            // Lấy SV hiện tại để loại trừ
            var mssvHienTai = HttpContext.Session.GetString("UserCode");

            keyword = keyword.Trim().ToLower();

            // Tìm SV đã được duyệt nguyện vọng và chưa có đề tài
            var danhSachSvDaCoDeTai = await _context.SinhVienDeTais
                .Where(svdt => svdt.IdDeTaiNavigation!.IdDot == dot.Id)
                .Select(svdt => svdt.IdSinhVien)
                .ToListAsync();

            var danhSachSv = await _context.DangKyNguyenVongs
                .Include(dk => dk.IdSinhVienNavigation)
                    .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Where(dk => dk.IdDot == dot.Id 
                    && dk.TrangThai == 1 // Đã được duyệt
                    && dk.IdSinhVienNavigation != null
                    && dk.IdSinhVienNavigation.Mssv != mssvHienTai // Loại trừ SV hiện tại
                    && !danhSachSvDaCoDeTai.Contains(dk.IdSinhVien) // Chưa có đề tài
                    && (dk.IdSinhVienNavigation.Mssv!.ToLower().Contains(keyword)
                        || dk.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen!.ToLower().Contains(keyword)))
                .Take(10)
                .Select(dk => new
                {
                    idSinhVien = dk.IdSinhVien,
                    mssv = dk.IdSinhVienNavigation!.Mssv,
                    hoTen = dk.IdSinhVienNavigation.IdNguoiDungNavigation!.HoTen
                })
                .ToListAsync();

            return Json(new { success = true, data = danhSachSv });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDangKy(int idDeTai)
        {
            var dot = await GetDotDoAnActive();
            if (dot == null)
                return Json(new { success = false, message = "Không tìm thấy đợt đồ án." });

            var giaiDoan = XacDinhGiaiDoanDangKy(dot);
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

        private static string XacDinhGiaiDoanDangKy(DotDoAn dot)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Kiểm tra giai đoạn đăng ký đề tài (sau khi đề xuất đề tài kết thúc)
            if (dot.NgayBatDauDkDeTai == null || dot.NgayKetThucDkDeTai == null)
                return "CHUA_MO";

            if (today < dot.NgayBatDauDkDeTai)
                return "CHUA_MO";

            if (today > dot.NgayKetThucDkDeTai)
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
