using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyHoiDongDuyetDeTaiController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyHoiDongDuyetDeTaiController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // Danh sách hội đồng
        public IActionResult Index(int? page, int? dotId, int? namHoc, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            //  Load Dropdown
            ViewBag.ListDot = new SelectList(_context.DotDoAns.OrderByDescending(d => d.Id), "Id", "TenDot", dotId);

            var listNamHoc = _context.HocKis
                .Select(h => new { h.NamBatDau, TenNam = $"{h.NamBatDau}-{h.NamKetThuc}" })
                .Distinct()
                .OrderByDescending(n => n.NamBatDau)
                .ToList();
            ViewBag.ListNamHoc = new SelectList(listNamHoc, "NamBatDau", "TenNam", namHoc);

            ViewBag.CurrentDotId = dotId;
            ViewBag.CurrentNamHoc = namHoc;
            ViewBag.CurrentFilter = searchString;

            // 2. Query dữ liệu từ bảng HoiDongBaoCao
            var query = _context.HoiDongBaoCaos
                .Include(hd => hd.IdBoMonNavigation)
                .Include(hd => hd.IdNguoiTaoNavigation)
                .Include(hd => hd.IdDotNavigation)
                .ThenInclude(d => d.IdHocKiNavigation)

                .Where(hd => hd.LoaiHoiDong == "DUYET_DE_TAI" || hd.LoaiHoiDong == "CUOI_KY" || hd.LoaiHoiDong == null)
                .AsQueryable();

            // 3. Áp dụng bộ lọc
            if (dotId.HasValue)
            {
                query = query.Where(hd => hd.IdDot == dotId);
            }

            if (namHoc.HasValue)
            {
                query = query.Where(hd => hd.IdDotNavigation.IdHocKiNavigation.NamBatDau == namHoc);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(hd => hd.MaHoiDong.Contains(searchString) || hd.TenHoiDong.Contains(searchString));
            }

            // 4. Map sang ViewModel
            var modelQuery = query.Select(hd => new QuanLyHoiDongViewModel
            {
                Id = hd.Id,
                MaHoiDong = hd.MaHoiDong,
                TenHoiDong = hd.TenHoiDong,
                TenBoMon = hd.IdBoMonNavigation.TenBoMon,
                NguoiTao = hd.IdNguoiTaoNavigation.HoTen,
                NgayBaoCao = hd.NgayBaoCao.HasValue ? hd.NgayBaoCao.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                DiaDiem = hd.DiaDiem,
                TrangThai = hd.TrangThai ?? false
            });

            // 5. Phân trang
            var pagedList = modelQuery.OrderByDescending(x => x.Id).ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }

        //  Duyệt hội đồng
        [HttpPost]
        public async Task<IActionResult> DuyetHoiDong(int id)
        {
            var hd = await _context.HoiDongBaoCaos.FindAsync(id);
            if (hd == null) return Json(new { success = false, message = "Không tìm thấy!" });

            hd.TrangThai = true; 
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã duyệt hội đồng thành công!" });
        }
    }
}