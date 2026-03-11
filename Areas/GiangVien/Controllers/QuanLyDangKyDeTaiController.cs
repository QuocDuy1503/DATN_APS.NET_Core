using DATN_TMS.Areas.GiangVien.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLyDangKyDeTaiController : Controller
    {
        private const int MAX_SV_PER_DETAI = 2;
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDangKyDeTaiController(QuanLyDoAnTotNghiepContext context)
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

        [HttpGet]
        public async Task<IActionResult> Index(int? page, string? chuyenNganh, string? searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentChuyenNganh = chuyenNganh;
            ViewBag.CurrentFilter = searchString;

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);

            if (giangVien == null)
            {
                ViewBag.ChuyenNganhs = new List<string>();
                return View(new List<DangKyDeTaiGVItem>().ToPagedList(1, pageSize));
            }

            var baseQuery = _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.SinhVienDeTais)
                .Where(dt => dt.IdGvhd == giangVien.IdNguoiDung);

            var allChuyenNganhs = await baseQuery
                .Where(dt => dt.IdChuyenNganhNavigation != null)
                .Select(dt => dt.IdChuyenNganhNavigation!.TenChuyenNganh)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
            ViewBag.ChuyenNganhs = allChuyenNganhs;

            var query = baseQuery.AsQueryable();

            if (!string.IsNullOrEmpty(chuyenNganh))
            {
                query = query.Where(dt => dt.IdChuyenNganhNavigation != null && dt.IdChuyenNganhNavigation.TenChuyenNganh == chuyenNganh);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dt => (dt.MaDeTai != null && dt.MaDeTai.Contains(searchString))
                    || (dt.TenDeTai != null && dt.TenDeTai.Contains(searchString)));
            }

            var data = query
                .OrderByDescending(dt => dt.Id)
                .Select(dt => new DangKyDeTaiGVItem
                {
                    IdDeTai = dt.Id,
                    MaDeTai = dt.MaDeTai,
                    TenDeTai = dt.TenDeTai,
                    ChuyenNganh = dt.IdChuyenNganhNavigation != null ? dt.IdChuyenNganhNavigation.TenChuyenNganh : "",
                    SoLuongDangKy = dt.SinhVienDeTais.Count
                });

            var pagedList = data.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null) return RedirectToAction("Index");

            var deTai = await _context.DeTais
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Include(dt => dt.IdDotNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdNguoiDungNavigation)
                .Include(dt => dt.SinhVienDeTais)
                    .ThenInclude(svdt => svdt.IdSinhVienNavigation)
                        .ThenInclude(sv => sv!.IdKhoaHocNavigation)
                .FirstOrDefaultAsync(dt => dt.Id == id && dt.IdGvhd == giangVien.IdNguoiDung);

            if (deTai == null) return NotFound();

            // Kiểm tra giai đoạn duyệt đăng ký
            var dot = deTai.IdDotNavigation;
            var today = DateOnly.FromDateTime(DateTime.Now);
            var coTheDuyet = true;
            string? thongBaoGiaiDoan = null;

            if (dot != null)
            {
                // Giảng viên có thể duyệt trong và sau giai đoạn đăng ký đề tài
                if (dot.NgayBatDauDkDeTai.HasValue && today < dot.NgayBatDauDkDeTai.Value)
                {
                    thongBaoGiaiDoan = $"Giai đoạn đăng ký đề tài chưa bắt đầu. Bắt đầu từ {dot.NgayBatDauDkDeTai.Value:dd/MM/yyyy}";
                }
            }

            var allDangKy = deTai.SinhVienDeTais
                .OrderBy(svdt => svdt.NgayDangKy)
                .ToList();
            var tongDaDuyet = allDangKy.Count(x => x.TrangThai == "DA_DUYET" || x.TrangThai == "Đã duyệt");

            // Lấy danh sách ID sinh viên để query GPA
            var sinhVienIds = allDangKy.Select(x => x.IdSinhVien).Where(x => x.HasValue).Select(x => x!.Value).ToList();
            var ketQuaHocTaps = await _context.KetQuaHocTaps
                .Where(kq => sinhVienIds.Contains(kq.IdSinhVien ?? 0))
                .GroupBy(kq => kq.IdSinhVien)
                .Select(g => new { IdSinhVien = g.Key, GPA = g.Max(x => x.Gpa), TongTinChi = g.Max(x => x.TongSoTinChi) })
                .ToListAsync();

            // Nhóm sinh viên theo thời điểm đăng ký (cùng giây = cùng nhóm)
            var danhSachNhom = new List<NhomDangKyGVItem>();
            var daXuLy = new HashSet<int>();

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
                        if (diff < 5) // Cùng nhóm nếu đăng ký trong vòng 5 giây
                        {
                            sv2 = svCandidate;
                            daXuLy.Add(svCandidate.Id);
                            break;
                        }
                    }
                }

                var kq1 = ketQuaHocTaps.FirstOrDefault(k => k.IdSinhVien == sv1.IdSinhVien);
                var kq2 = sv2 != null ? ketQuaHocTaps.FirstOrDefault(k => k.IdSinhVien == sv2.IdSinhVien) : null;

                danhSachNhom.Add(new NhomDangKyGVItem
                {
                    IdSvDeTai1 = sv1.Id,
                    IdSinhVien1 = sv1.IdSinhVien,
                    Mssv1 = sv1.IdSinhVienNavigation?.Mssv,
                    HoTen1 = sv1.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    Email1 = sv1.IdSinhVienNavigation?.IdNguoiDungNavigation?.Email,
                    KhoaHoc1 = sv1.IdSinhVienNavigation?.IdKhoaHocNavigation?.TenKhoa,
                    TinChiTichLuy1 = sv1.IdSinhVienNavigation?.TinChiTichLuy ?? kq1?.TongTinChi,
                    GPA1 = kq1?.GPA,

                    IdSvDeTai2 = sv2?.Id,
                    IdSinhVien2 = sv2?.IdSinhVien,
                    Mssv2 = sv2?.IdSinhVienNavigation?.Mssv,
                    HoTen2 = sv2?.IdSinhVienNavigation?.IdNguoiDungNavigation?.HoTen,
                    Email2 = sv2?.IdSinhVienNavigation?.IdNguoiDungNavigation?.Email,
                    KhoaHoc2 = sv2?.IdSinhVienNavigation?.IdKhoaHocNavigation?.TenKhoa,
                    TinChiTichLuy2 = sv2?.IdSinhVienNavigation?.TinChiTichLuy ?? kq2?.TongTinChi,
                    GPA2 = kq2?.GPA,

                    TrangThai = sv1.TrangThai,
                    StatusCss = GetStatusCss(sv1.TrangThai),
                    NgayDangKy = sv1.NgayDangKy?.ToString("dd/MM/yyyy HH:mm"),
                    NhanXet = sv1.NhanXet
                });
            }

            var vm = new ChiTietDangKyDeTaiGVViewModel
            {
                IdDeTai = deTai.Id,
                MaDeTai = deTai.MaDeTai,
                TenDeTai = deTai.TenDeTai,
                TenChuyenNganh = deTai.IdChuyenNganhNavigation?.TenChuyenNganh,
                NguoiDeXuat = deTai.IdNguoiDeXuatNavigation?.HoTen,
                MucTieu = deTai.MucTieuChinh,
                YeuCauTinhMoi = deTai.YeuCauTinhMoi,
                PhamVi = deTai.PhamViChucNang,
                CongNghe = deTai.CongNgheSuDung,
                KetQuaDuKien = deTai.SanPhamKetQuaDuKien,
                NhiemVuCuThe = deTai.NhiemVuCuThe,
                SoLuongNhom = danhSachNhom.Count,
                SoLuongDaDuyet = danhSachNhom.Count(n => n.TrangThai == "DA_DUYET" || n.TrangThai == "Đã duyệt"),
                CoTheDuyet = coTheDuyet,
                ThongBaoGiaiDoan = thongBaoGiaiDoan,
                DanhSachNhom = danhSachNhom
                    .OrderBy(n => n.TrangThai == "DA_DUYET" || n.TrangThai == "Đã duyệt" ? 0 : n.TrangThai == "Chờ GVHD duyệt" ? 1 : 2)
                    .ToList()
            };

            return View(vm);
        }

        // API lấy kết quả học tập của sinh viên
        [HttpGet]
        public async Task<IActionResult> GetKetQuaHocTap(int idSinhVien)
        {
            var ketQua = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == idSinhVien)
                .OrderBy(kq => kq.Stt)
                .Select(kq => new
                {
                    stt = kq.Stt,
                    maHocPhan = kq.MaHocPhan,
                    tenHocPhan = kq.TenHocPhan,
                    soTinChi = kq.SoTc,
                    diemSo = kq.DiemSo,
                    diemChu = kq.DiemChu,
                    ketQua = kq.KetQua == true ? "Đạt" : "Không đạt"
                })
                .ToListAsync();

            // Lấy tổng kết
            var tongKet = await _context.KetQuaHocTaps
                .Where(kq => kq.IdSinhVien == idSinhVien)
                .GroupBy(kq => kq.IdSinhVien)
                .Select(g => new
                {
                    tongTinChi = g.Max(x => x.TongSoTinChi),
                    gpa = g.Max(x => x.Gpa)
                })
                .FirstOrDefaultAsync();

            return Json(new
            {
                success = true,
                data = ketQua,
                tongKet = new
                {
                    tongTinChi = tongKet?.tongTinChi ?? 0,
                    gpa = tongKet?.gpa ?? 0
                }
            });
        }

        private string GetStatusCss(string? trangThai)
        {
            return trangThai switch
            {
                "DA_DUYET" or "Đã duyệt" => "member-approved",
                "Chờ GVHD duyệt" => "member-pending",
                "TU_CHOI" or "Từ chối" => "member-rejected",
                _ => "member-default"
            };
        }

        // Duyệt cả nhóm (1 hoặc 2 SV)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetNhom(int idSvDeTai1, int? idSvDeTai2, string action, string? nhanXet = null)
        {
            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null)
                return Json(new { success = false, message = "Không tìm thấy thông tin giảng viên." });

            // Lấy SV đầu tiên với thông tin liên quan
            var svDeTai1 = await _context.SinhVienDeTais
                .Include(s => s.IdSinhVienNavigation)
                .FirstOrDefaultAsync(s => s.Id == idSvDeTai1);
            if (svDeTai1 == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi đăng ký." });

            var deTai = await _context.DeTais.FindAsync(svDeTai1.IdDeTai);
            if (deTai == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền duyệt sinh viên cho đề tài này." });

            // Lấy SV thứ 2 (nếu có)
            SinhVienDeTai? svDeTai2 = null;
            if (idSvDeTai2.HasValue && idSvDeTai2.Value > 0)
            {
                svDeTai2 = await _context.SinhVienDeTais
                    .Include(s => s.IdSinhVienNavigation)
                    .FirstOrDefaultAsync(s => s.Id == idSvDeTai2.Value);
            }

            if (action == "approve")
            {
                // Kiểm tra số lượng đã duyệt
                var soSvDaDuyet = await _context.SinhVienDeTais
                    .CountAsync(s => s.IdDeTai == deTai.Id && (s.TrangThai == "DA_DUYET" || s.TrangThai == "Đã duyệt"));

                var soSvCanDuyet = svDeTai2 != null ? 2 : 1;
                if (soSvDaDuyet + soSvCanDuyet > MAX_SV_PER_DETAI)
                    return Json(new { success = false, message = $"Đề tài đã đủ số lượng sinh viên (tối đa {MAX_SV_PER_DETAI})." });

                // Duyệt sinh viên
                svDeTai1.TrangThai = "Đã duyệt";
                if (svDeTai2 != null) svDeTai2.TrangThai = "Đã duyệt";

                // Gửi thông báo cho SV được duyệt
                await GuiThongBaoDuyet(svDeTai1.IdSinhVien, deTai.TenDeTai, deTai.MaDeTai);
                if (svDeTai2 != null)
                {
                    await GuiThongBaoDuyet(svDeTai2.IdSinhVien, deTai.TenDeTai, deTai.MaDeTai);
                }

                // Kiểm tra nếu đề tài đã đủ người, từ chối các SV còn lại
                var tongSvSauDuyet = soSvDaDuyet + soSvCanDuyet;
                if (tongSvSauDuyet >= MAX_SV_PER_DETAI)
                {
                    // Lấy danh sách ID đã duyệt
                    var dsDaDuyet = new List<int> { svDeTai1.Id };
                    if (svDeTai2 != null) dsDaDuyet.Add(svDeTai2.Id);

                    // Từ chối các SV còn lại chưa được duyệt
                    var svConLai = await _context.SinhVienDeTais
                        .Where(s => s.IdDeTai == deTai.Id 
                            && !dsDaDuyet.Contains(s.Id)
                            && s.TrangThai != "DA_DUYET" 
                            && s.TrangThai != "Đã duyệt"
                            && s.TrangThai != "TU_CHOI"
                            && s.TrangThai != "Từ chối")
                        .ToListAsync();

                    foreach (var sv in svConLai)
                    {
                        sv.TrangThai = "Từ chối";
                        sv.NhanXet = "Đề tài đã đủ số lượng sinh viên.";

                        // Gửi thông báo cho SV bị từ chối vì đề tài đủ người
                        await GuiThongBaoDeTaiDuNguoi(sv.IdSinhVien, deTai.TenDeTai, deTai.MaDeTai);
                    }
                }
            }
            else if (action == "reject")
            {
                svDeTai1.TrangThai = "Từ chối";
                svDeTai1.NhanXet = nhanXet;

                // Gửi thông báo từ chối
                await GuiThongBaoTuChoi(svDeTai1.IdSinhVien, deTai.TenDeTai, deTai.MaDeTai, nhanXet);

                if (svDeTai2 != null) 
                {
                    svDeTai2.TrangThai = "Từ chối";
                    svDeTai2.NhanXet = nhanXet;
                    await GuiThongBaoTuChoi(svDeTai2.IdSinhVien, deTai.TenDeTai, deTai.MaDeTai, nhanXet);
                }
            }
            else
            {
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            await _context.SaveChangesAsync();
            var msg = action == "approve" 
                ? (svDeTai2 != null ? "Đã duyệt nhóm thành công!" : "Đã duyệt sinh viên thành công!")
                : (svDeTai2 != null ? "Đã từ chối nhóm." : "Đã từ chối sinh viên.");
            return Json(new { success = true, message = msg });
        }

        // Giữ lại API cũ cho backward compatibility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetSinhVien(int idSvDeTai, string action, string? nhanXet = null)
        {
            var svDeTai = await _context.SinhVienDeTais.FindAsync(idSvDeTai);
            if (svDeTai == null)
                return Json(new { success = false, message = "Không tìm thấy bản ghi đăng ký." });

            var deTai = await _context.DeTais.FindAsync(svDeTai.IdDeTai);
            if (deTai == null)
                return Json(new { success = false, message = "Không tìm thấy đề tài." });

            var maGV = HttpContext.Session.GetString("UserCode");
            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.MaGv == maGV);
            if (giangVien == null || deTai.IdGvhd != giangVien.IdNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền duyệt sinh viên cho đề tài này." });

            if (action == "approve")
            {
                var soSvDaDuyet = await _context.SinhVienDeTais
                    .CountAsync(s => s.IdDeTai == deTai.Id && (s.TrangThai == "DA_DUYET" || s.TrangThai == "Đã duyệt"));
                if (soSvDaDuyet >= MAX_SV_PER_DETAI)
                    return Json(new { success = false, message = $"Đề tài đã đủ số lượng sinh viên (tối đa {MAX_SV_PER_DETAI})." });

                svDeTai.TrangThai = "Đã duyệt";
            }
            else if (action == "reject")
            {
                svDeTai.TrangThai = "Từ chối";
                svDeTai.NhanXet = nhanXet;
            }
            else
            {
                return Json(new { success = false, message = "Hành động không hợp lệ." });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = action == "approve" ? "Duyệt thành công!" : "Đã từ chối." });
        }

        // Gửi thông báo khi duyệt sinh viên vào đề tài
        private async Task GuiThongBaoDuyet(int? idSinhVien, string? tenDeTai, string? maDeTai)
        {
            if (!idSinhVien.HasValue) return;

            var thongBao = new DATN_TMS.Models.ThongBao
            {
                IdNguoiNhan = idSinhVien.Value,
                TieuDe = "Đăng ký đề tài được duyệt",
                NoiDung = $"Bạn đã được duyệt vào đề tài [{maDeTai}] - {tenDeTai}. Hãy liên hệ giảng viên hướng dẫn để bắt đầu thực hiện đồ án.",
                LinkLienKet = "/SinhVien/DangKyDeTai",
                TrangThaiXem = false,
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();
        }

        // Gửi thông báo khi từ chối sinh viên
        private async Task GuiThongBaoTuChoi(int? idSinhVien, string? tenDeTai, string? maDeTai, string? lyDo)
        {
            if (!idSinhVien.HasValue) return;

            var thongBao = new DATN_TMS.Models.ThongBao
            {
                IdNguoiNhan = idSinhVien.Value,
                TieuDe = "Đăng ký đề tài bị từ chối",
                NoiDung = $"Đăng ký của bạn vào đề tài [{maDeTai}] - {tenDeTai} đã bị từ chối. Lý do: {lyDo ?? "Không phù hợp"}. Bạn có thể đăng ký đề tài khác.",
                LinkLienKet = "/SinhVien/DangKyDeTai",
                TrangThaiXem = false,
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();
        }

        // Gửi thông báo khi đề tài đã đủ người
        private async Task GuiThongBaoDeTaiDuNguoi(int? idSinhVien, string? tenDeTai, string? maDeTai)
        {
            if (!idSinhVien.HasValue) return;

            var thongBao = new DATN_TMS.Models.ThongBao
            {
                IdNguoiNhan = idSinhVien.Value,
                TieuDe = "Đề tài đã đủ số lượng sinh viên",
                NoiDung = $"Đề tài [{maDeTai}] - {tenDeTai} mà bạn đăng ký đã đủ số lượng sinh viên. Vui lòng chọn đề tài khác để đăng ký.",
                LinkLienKet = "/SinhVien/DangKyDeTai",
                TrangThaiXem = false,
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();
        }
    }
}
