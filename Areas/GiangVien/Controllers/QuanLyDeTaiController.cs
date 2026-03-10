using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class QuanLyDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionRole = HttpContext.Session.GetString("Role");
            var isGiangVienByClaim = User?.Identity?.IsAuthenticated == true && 
                (User.IsInRole("GIANG_VIEN") || User.IsInRole("BO_MON") || User.IsInRole("BCN_KHOA"));
            var isGiangVienBySession = sessionRole == "GIANG_VIEN" || sessionRole == "BO_MON" || sessionRole == "BCN_KHOA";

            if (!isGiangVienByClaim && !isGiangVienBySession)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "" });
                return;
            }

            base.OnActionExecuting(context);
        }

        // Danh sách đề tài - Chỉ hiển thị các đợt đã kết thúc giai đoạn đề xuất đề tài
        public async Task<IActionResult> Index(int? page, int? dotId, int? namHoc, int? chuyenNganhId, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;
            var today = DateOnly.FromDateTime(DateTime.Now);

            // Chỉ lấy các đợt đã kết thúc giai đoạn đề xuất đề tài
            var availableDots = await _context.DotDoAns
                .Where(d => d.NgayKetThucDeXuatDeTai.HasValue && d.NgayKetThucDeXuatDeTai.Value < today)
                .OrderByDescending(d => d.Id)
                .ToListAsync();

            ViewBag.ListDot = new SelectList(availableDots, "Id", "TenDot", dotId);

            var listNamHoc = _context.HocKis
                .Select(h => new { h.NamBatDau, TenNam = $"{h.NamBatDau}-{h.NamKetThuc}" })
                .Distinct()
                .OrderByDescending(n => n.NamBatDau)
                .ToList();
            ViewBag.ListNamHoc = new SelectList(listNamHoc, "NamBatDau", "TenNam", namHoc);

            ViewBag.ListChuyenNganh = new SelectList(_context.ChuyenNganhs, "Id", "TenChuyenNganh", chuyenNganhId);

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentNamHoc = namHoc;
            ViewBag.CurrentChuyenNganh = chuyenNganhId;
            ViewBag.CurrentFilter = searchString;

            // Lấy danh sách ID các đợt có thể xem
            var availableDotIds = availableDots.Select(d => d.Id).ToList();

            var query = _context.DeTais
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv.IdNguoiDungNavigation)
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdDotNavigation)
                    .ThenInclude(d => d.IdHocKiNavigation)
                .Where(dt => dt.IdDot.HasValue && availableDotIds.Contains(dt.IdDot.Value))
                .AsQueryable();

            if (dotId.HasValue)
            {
                query = query.Where(dt => dt.IdDot == dotId);
            }

            if (namHoc.HasValue)
            {
                query = query.Where(dt => dt.IdDotNavigation.IdHocKiNavigation.NamBatDau == namHoc);
            }

            if (chuyenNganhId.HasValue)
            {
                query = query.Where(dt => dt.IdChuyenNganh == chuyenNganhId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(dt => dt.MaDeTai.Contains(searchString) || dt.TenDeTai.Contains(searchString));
            }

            var modelQuery = query.Select(dt => new QuanLyDeTaiViewModel
            {
                Id = dt.Id,
                MaDeTai = dt.MaDeTai,
                TenDeTai = dt.TenDeTai,
                NguoiDeXuat = dt.IdNguoiDeXuatNavigation.HoTen,
                GVHD = dt.IdGvhdNavigation.IdNguoiDungNavigation.HoTen,
                TenChuyenNganh = dt.IdChuyenNganhNavigation.TenChuyenNganh,
                TrangThai = dt.TrangThai
            });

            var pagedList = modelQuery.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        // Chi tiết đề tài
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var today = DateOnly.FromDateTime(DateTime.Now);

            var detai = await _context.DeTais
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Include(dt => dt.IdGvhdNavigation).ThenInclude(gv => gv.IdNguoiDungNavigation)
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdDotNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (detai == null) return NotFound();

            // Kiểm tra xem đợt đã kết thúc giai đoạn đề xuất chưa
            if (!detai.IdDotNavigation?.NgayKetThucDeXuatDeTai.HasValue == true ||
                detai.IdDotNavigation.NgayKetThucDeXuatDeTai.Value >= today)
            {
                TempData["ErrorMessage"] = "Đợt này chưa kết thúc giai đoạn đề xuất đề tài.";
                return RedirectToAction(nameof(Index));
            }

            var currentEmail = HttpContext.Session.GetString("UserEmail");
            var currentUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == currentEmail);
            var currentGiangVien = currentUser != null
                ? await _context.GiangViens.FirstOrDefaultAsync(gv => gv.IdNguoiDung == currentUser.Id)
                : null;

            // Kiểm tra xem giảng viên có thuộc hội đồng duyệt đề tài không
            var hoiDong = await _context.HoiDongBaoCaos
                .Include(h => h.ThanhVienHdBaoCaos)
                    .ThenInclude(tv => tv.IdGiangVienNavigation)
                        .ThenInclude(gv => gv.IdNguoiDungNavigation)
                .FirstOrDefaultAsync(h => h.IdDot == detai.IdDot && h.LoaiHoiDong == "DUYET_DE_TAI");

            var councilMemberIds = hoiDong?.ThanhVienHdBaoCaos.Select(tv => tv.IdGiangVien ?? 0).ToList() ?? new List<int>();
            bool isCouncilMember = currentGiangVien != null && councilMemberIds.Contains(currentGiangVien.IdNguoiDung);
            bool isProposer = currentUser != null && currentUser.Id == detai.IdNguoiDeXuat;

            var reviews = await _context.NhanXetHoiDongDeTais
                .Include(r => r.GiangVien).ThenInclude(gv => gv.IdNguoiDungNavigation)
                .Where(r => r.IdDeTai == detai.Id)
                .ToListAsync();

            var currentReview = currentGiangVien != null
                ? reviews.FirstOrDefault(r => r.IdGiangVien == currentGiangVien.IdNguoiDung)
                : null;

            // Tìm sinh viên đã đăng ký đề tài này
            var nhomSV = await _context.SinhVienDeTais
                .Include(svdt => svdt.IdSinhVienNavigation).ThenInclude(sv => sv.IdNguoiDungNavigation)
                .Where(svdt => svdt.IdDeTai == id)
                .Select(svdt => svdt.IdSinhVienNavigation.IdNguoiDungNavigation.HoTen)
                .ToListAsync();

            var model = new ChiTietDeTaiViewModel
            {
                Id = detai.Id,
                MaDeTai = detai.MaDeTai,
                TenDeTai = detai.TenDeTai,
                NguoiDeXuat = detai.IdNguoiDeXuatNavigation?.HoTen,
                GVHD = detai.IdGvhdNavigation?.IdNguoiDungNavigation?.HoTen,
                TenChuyenNganh = detai.IdChuyenNganhNavigation?.TenChuyenNganh,

                MucTieu = detai.MucTieuChinh,
                PhamVi = detai.PhamViChucNang,
                CongNghe = detai.CongNgheSuDung,
                YeuCauTinhMoi = detai.YeuCauTinhMoi,
                KetQuaDuKien = detai.SanPhamKetQuaDuKien,

                NhomThucHien = nhomSV.Any() ? string.Join(", ", nhomSV) : "Chưa có nhóm đăng ký",

                TrangThai = detai.TrangThai,
                NhanXet = currentReview?.NhanXet ?? string.Empty,
                CanReview = isCouncilMember,
                IsProposer = isProposer,
                CurrentUserStatus = currentReview?.TrangThai ?? string.Empty,
                CouncilMemberCount = councilMemberIds.Count,
                Reviews = reviews.Select(r => new ReviewItem
                {
                    ReviewerName = r.GiangVien?.IdNguoiDungNavigation?.HoTen ?? "Giảng viên",
                    Comment = r.NhanXet,
                    Status = r.TrangThai ?? "CHO_DUYET",
                    CreatedAt = r.NgayTao,
                    IsCurrentUser = currentGiangVien != null && r.IdGiangVien == currentGiangVien.IdNguoiDung
                }).ToList()
            };

            return View(model);
        }

        // POST: Duyệt đề tài 
        [HttpPost]
        public async Task<IActionResult> DuyetDeTai(int id, string nhanXet)
        {
            return await SaveReview(id, nhanXet, "DA_DUYET");
        }

        // POST: Từ chối đề tài 
        [HttpPost]
        public async Task<IActionResult> TuChoiDeTai(int id, string nhanXet)
        {
            if (string.IsNullOrWhiteSpace(nhanXet))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do từ chối!" });
            }

            return await SaveReview(id, nhanXet, "TU_CHOI");
        }

        private async Task<JsonResult> SaveReview(int id, string nhanXet, string status)
        {
            var detai = await _context.DeTais.FindAsync(id);
            if (detai == null) return Json(new { success = false, message = "Không tìm thấy đề tài!" });

            var currentEmail = HttpContext.Session.GetString("UserEmail");
            var currentUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == currentEmail);
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Phiên đăng nhập hết hạn!" });
            }

            var giangVien = await _context.GiangViens.FirstOrDefaultAsync(gv => gv.IdNguoiDung == currentUser.Id);
            if (giangVien == null)
            {
                return Json(new { success = false, message = "Chỉ thành viên hội đồng mới được xét duyệt." });
            }

            var hoiDong = await _context.HoiDongBaoCaos
                .Include(h => h.ThanhVienHdBaoCaos)
                .FirstOrDefaultAsync(h => h.IdDot == detai.IdDot && h.LoaiHoiDong == "DUYET_DE_TAI");

            if (hoiDong == null || !hoiDong.ThanhVienHdBaoCaos.Any(tv => tv.IdGiangVien == giangVien.IdNguoiDung))
            {
                return Json(new { success = false, message = "Bạn không thuộc hội đồng duyệt đề tài này." });
            }

            if (detai.IdNguoiDeXuat == currentUser.Id)
            {
                return Json(new { success = false, message = "Người đề xuất không được tự duyệt." });
            }

            var review = await _context.NhanXetHoiDongDeTais
                .FirstOrDefaultAsync(r => r.IdDeTai == detai.Id && r.IdGiangVien == giangVien.IdNguoiDung);

            if (review == null)
            {
                review = new NhanXetHoiDongDeTai
                {
                    IdDeTai = detai.Id,
                    IdGiangVien = giangVien.IdNguoiDung,
                    NgayTao = DateTime.Now
                };
                _context.NhanXetHoiDongDeTais.Add(review);
            }

            review.TrangThai = status;
            review.NhanXet = nhanXet;
            review.NgayTao = DateTime.Now;

            await _context.SaveChangesAsync();

            await UpdateOverallStatus(detai, hoiDong);

            // Lấy số lượng nhận xét hiện tại
            var totalMembers = hoiDong?.ThanhVienHdBaoCaos?.Count ?? 0;
            var reviewCount = await _context.NhanXetHoiDongDeTais.CountAsync(r => r.IdDeTai == detai.Id);

            return Json(new { 
                success = true, 
                message = status == "DA_DUYET" ? "Đã ghi nhận phê duyệt." : "Đã ghi nhận từ chối.",
                overallStatus = detai.TrangThai,
                reviewCount = reviewCount,
                totalMembers = totalMembers
            });
        }

        private async Task UpdateOverallStatus(DeTai detai, HoiDongBaoCao hoiDong)
        {
            var totalMembers = hoiDong?.ThanhVienHdBaoCaos?.Count ?? 0;
            var reviews = await _context.NhanXetHoiDongDeTais.Where(r => r.IdDeTai == detai.Id).ToListAsync();

            var previousStatus = detai.TrangThai;

            if (reviews.Any(r => r.TrangThai == "TU_CHOI"))
            {
                detai.TrangThai = "TU_CHOI";
            }
            else if (totalMembers > 0 && reviews.Count >= totalMembers && reviews.All(r => r.TrangThai == "DA_DUYET"))
            {
                detai.TrangThai = "DA_DUYET";
            }
            else
            {
                detai.TrangThai = "CHO_DUYET";
            }

            await _context.SaveChangesAsync();

            if (detai.IdNguoiDeXuat.HasValue && !string.Equals(previousStatus, detai.TrangThai, StringComparison.OrdinalIgnoreCase))
            {
                if (detai.TrangThai == "TU_CHOI")
                {
                    await CreateThongBao(detai.IdNguoiDeXuat.Value, "Đề tài bị từ chối", $"Đề tài {detai.MaDeTai} đã bị từ chối bởi hội đồng.", $"/GiangVien/QuanLyDeTai/Details/{detai.Id}");
                }
                else if (detai.TrangThai == "DA_DUYET")
                {
                    await CreateThongBao(detai.IdNguoiDeXuat.Value, "Đề tài đã được duyệt", $"Đề tài {detai.MaDeTai} đã được hội đồng phê duyệt.", $"/GiangVien/QuanLyDeTai/Details/{detai.Id}");
                }
            }
        }

        private async Task CreateThongBao(int idNguoiNhan, string tieuDe, string noiDung, string link)
        {
            var thongBao = new ThongBao
            {
                IdNguoiNhan = idNguoiNhan,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                LinkLienKet = link,
                TrangThaiXem = false,
                NgayTao = DateTime.Now
            };
            _context.ThongBaos.Add(thongBao);
            await _context.SaveChangesAsync();
        }
    }
}
