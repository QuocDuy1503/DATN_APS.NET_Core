using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Areas.BCNKhoa.Models.ViewModels;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // Danh sách đề tài
        public IActionResult Index(int? page, int? dotId, int? namHoc, int? chuyenNganhId, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // List Đợt
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);

            // List Năm học
            var listNamHoc = _context.HocKis
                .Select(h => new { h.NamBatDau, TenNam = $"{h.NamBatDau}-{h.NamKetThuc}" })
                .Distinct()
                .OrderByDescending(n => n.NamBatDau)
                .ToList();
            ViewBag.ListNamHoc = new SelectList(listNamHoc, "NamBatDau", "TenNam", namHoc);

            // List Chuyên ngành
            ViewBag.ListChuyenNganh = new SelectList(_context.ChuyenNganhs, "Id", "TenChuyenNganh", chuyenNganhId);

            // Lưu trạng thái
            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentNamHoc = namHoc;
            ViewBag.CurrentChuyenNganh = chuyenNganhId;
            ViewBag.CurrentFilter = searchString;

            // 2. Query dữ liệu
            var query = _context.DeTais
                .Include(dt => dt.IdNguoiDeXuatNavigation) 
                .Include(dt => dt.IdGvhdNavigation)
                    .ThenInclude(gv => gv.IdNguoiDungNavigation) 
                .Include(dt => dt.IdChuyenNganhNavigation)
                .Include(dt => dt.IdDotNavigation) // Để lọc theo năm học qua Đợt -> Học kỳ
                    .ThenInclude(d => d.IdHocKiNavigation)
                .AsQueryable();

            // bộ lọc
            if (dotId.HasValue)
            {
                query = query.Where(dt => dt.IdDot == dotId);
            }

            if (namHoc.HasValue)
            {
                // Lọc đề tài thuộc đợt của năm học đó
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

            // Select ra ViewModel
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


        public async Task<IActionResult> Details(int id)
        {
            var detai = await _context.DeTais
                .Include(dt => dt.IdNguoiDeXuatNavigation)
                .Include(dt => dt.IdGvhdNavigation).ThenInclude(gv => gv.IdNguoiDungNavigation)
                .Include(dt => dt.IdChuyenNganhNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (detai == null) return NotFound();

            // Tìm sinh viên đã đăng ký đề tài này (nếu có) để hiển thị nhóm
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
                NhanXet = detai.NhanXetDuyet
            };

            return View(model);
        }

        // POST: Duyệt đề tài 
        [HttpPost]
        public async Task<IActionResult> DuyetDeTai(int id, string nhanXet)
        {
            var detai = await _context.DeTais.FindAsync(id);
            if (detai == null) return Json(new { success = false, message = "Không tìm thấy đề tài!" });

            try
            {
                detai.TrangThai = "DA_DUYET";
                detai.NhanXetDuyet = nhanXet; 
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã duyệt đề tài thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Từ chối đề tài 
        [HttpPost]
        public async Task<IActionResult> TuChoiDeTai(int id, string nhanXet)
        {
            var detai = await _context.DeTais.FindAsync(id);
            if (detai == null) return Json(new { success = false, message = "Không tìm thấy đề tài!" });

            if (string.IsNullOrWhiteSpace(nhanXet))
            {
                return Json(new { success = false, message = "Vui lòng nhập lý do từ chối!" });
            }

            try
            {
                detai.TrangThai = "TU_CHOI";
                detai.NhanXetDuyet = nhanXet;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã từ chối đề tài!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}