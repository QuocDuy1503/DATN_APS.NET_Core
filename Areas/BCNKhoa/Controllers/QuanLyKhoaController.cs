using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.BCNKhoa.Models;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyKhoaController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyKhoaController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            var query = _context.KhoaHocs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // Tìm theo Mã khóa hoặc Tên khóa
                query = query.Where(k => k.MaKhoa.Contains(searchString) ||
                                         k.TenKhoa.Contains(searchString));
            }

            // Sắp xếp: Năm nhập học giảm dần (Khóa mới nhất lên đầu)
            query = query.OrderByDescending(k => k.NamNhapHoc);

            var modelQuery = query.Select(k => new KhoaViewModel
            {
                Id = k.Id,
                MaKhoa = k.MaKhoa,
                TenKhoa = k.TenKhoa,
                NamNhapHoc = k.NamNhapHoc,
                NamTotNghiep = k.NamTotNghiep,
                TrangThai = k.TrangThai
            });

            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);
            return View(pagedList);
        }

        // GET: ChiTiet (API cho modal sửa)
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var khoa = await _context.KhoaHocs.FindAsync(id);
            if (khoa == null) return Json(new { success = false, message = "Không tìm thấy khóa." });
            return Json(new { success = true, data = khoa });
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string MaKhoa, string TenKhoa, int? NamNhapHoc, int? NamTotNghiep)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MaKhoa) || string.IsNullOrWhiteSpace(TenKhoa))
                {
                    TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ Mã khóa và Tên khóa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng Mã khóa
                if (await _context.KhoaHocs.AnyAsync(k => k.MaKhoa == MaKhoa))
                {
                    TempData["ErrorMessage"] = $"Mã khóa '{MaKhoa}' đã tồn tại.";
                    return RedirectToAction("Index");
                }

                var khoa = new KhoaHoc
                {
                    MaKhoa = MaKhoa.Trim(),
                    TenKhoa = TenKhoa.Trim(),
                    NamNhapHoc = NamNhapHoc,
                    NamTotNghiep = NamTotNghiep,
                    TrangThai = true // Mặc định là đang đào tạo
                };

                _context.KhoaHocs.Add(khoa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm khóa mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string MaKhoa, string TenKhoa, int? NamNhapHoc, int? NamTotNghiep, bool TrangThai)
        {
            try
            {
                var khoa = await _context.KhoaHocs.FindAsync(Id);
                if (khoa == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khóa.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra trùng Mã khóa (trừ bản ghi hiện tại)
                if (await _context.KhoaHocs.AnyAsync(k => k.MaKhoa == MaKhoa && k.Id != Id))
                {
                    TempData["ErrorMessage"] = $"Mã khóa '{MaKhoa}' đã tồn tại.";
                    return RedirectToAction("Index");
                }

                khoa.MaKhoa = MaKhoa.Trim();
                khoa.TenKhoa = TenKhoa.Trim();
                khoa.NamNhapHoc = NamNhapHoc;
                khoa.NamTotNghiep = NamTotNghiep;
                khoa.TrangThai = TrangThai;

                _context.KhoaHocs.Update(khoa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật khóa thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var khoa = await _context.KhoaHocs
                    .Include(k => k.SinhViens)
                    .Include(k => k.ChuongTrinhDaoTaos)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (khoa == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy dữ liệu.";
                    return RedirectToAction("Index");
                }

                // Kiểm tra ràng buộc dữ liệu
                if (khoa.SinhViens.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa khóa '{khoa.MaKhoa}' vì đã có {khoa.SinhViens.Count} sinh viên.";
                    return RedirectToAction("Index");
                }
                if (khoa.ChuongTrinhDaoTaos.Any())
                {
                    TempData["ErrorMessage"] = $"Không thể xóa vì đã có chương trình đào tạo liên kết.";
                    return RedirectToAction("Index");
                }

                _context.KhoaHocs.Remove(khoa);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa khóa thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}