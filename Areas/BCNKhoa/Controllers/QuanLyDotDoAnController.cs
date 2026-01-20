using DATN_TMS.Areas.BCNKhoa.Models.ViewModels;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyDotDoAnController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyDotDoAnController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }
        public IActionResult Index(int? page, int? khoaId, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var listKhoa = _context.KhoaHocs.OrderByDescending(k => k.Id).ToList();
            ViewBag.ListKhoa = new SelectList(listKhoa, "Id", "TenKhoa", khoaId);
            ViewBag.CurrentKhoaId = khoaId;
            var query = _context.DotDoAns
                .Include(d => d.IdKhoaHocNavigation) 
                .Include(d => d.IdHocKiNavigation)
                .AsQueryable();

            if (khoaId.HasValue && khoaId.Value > 0)
            {
                query = query.Where(d => d.IdKhoaHoc == khoaId);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(d => d.TenDot.Contains(searchString));
            }

            var result = query.OrderByDescending(d => d.Id).ToPagedList(pageNumber, pageSize);
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DotDoAnViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var dotDoAn = new DotDoAn
                    {
                        TenDot = model.TenDot,
                        IdKhoaHoc = int.TryParse(model.Khoa, out int idKhoa) ? idKhoa : null,
                        NgayBatDauDot = DateOnly.FromDateTime(model.NgayBatDau),
                        NgayKetThucDot = DateOnly.FromDateTime(model.NgayKetThuc),
                        TrangThai = true
                    };

                    _context.DotDoAns.Add(dotDoAn);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thêm đợt đồ án thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi lưu dữ liệu: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dotDoAn = await _context.DotDoAns.FindAsync(id);
            if (dotDoAn == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đợt đồ án!";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.DotDoAns.Remove(dotDoAn);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa đợt đồ án thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa (có thể do dữ liệu ràng buộc): " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị trang Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dotDoAn = await _context.DotDoAns.FindAsync(id);
            if (dotDoAn == null) return NotFound();

            // Lấy danh sách Khóa để đổ vào Dropdown
            ViewBag.ListKhoa = new SelectList(_context.KhoaHocs.OrderByDescending(k => k.Id), "Id", "TenKhoa", dotDoAn.IdKhoaHoc);

            return View(dotDoAn);
        }

        //Lưu thay đổi 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DotDoAn model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật dữ liệu
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật đợt đồ án thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.DotDoAns.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi hệ thống: " + ex.Message;
                }
            }
            var listKhoa = _context.KhoaHocs.OrderByDescending(k => k.Id).ToList();
            ViewBag.ListKhoa = new SelectList(_context.KhoaHocs.OrderByDescending(k => k.Id), "Id", "TenKhoa", model.IdKhoaHoc);
            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin!";
            return View(model);
        }

    }
}
