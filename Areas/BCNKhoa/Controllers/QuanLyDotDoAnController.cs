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
        public IActionResult Index(int? page, int? khoaId, string? namHoc, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            var listKhoa = _context.KhoaHocs.OrderByDescending(k => k.Id).ToList();
            ViewBag.ListKhoa = new SelectList(listKhoa, "Id", "TenKhoa", khoaId);
            ViewBag.CurrentKhoaId = khoaId;
            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentNamHoc = namHoc;

            var listNamHoc = _context.HocKis
                .GroupBy(h => new { h.NamBatDau, h.NamKetThuc })
                .OrderByDescending(g => g.Key.NamBatDau)
                .Select(g => new SelectListItem
                {
                    Value = $"{g.Key.NamBatDau}-{g.Key.NamKetThuc}",
                    Text = $"{g.Key.NamBatDau}-{g.Key.NamKetThuc}"
                })
                .ToList();
            ViewBag.ListNamHoc = listNamHoc;
            var query = _context.DotDoAns
                .Include(d => d.IdKhoaHocNavigation) 
                .Include(d => d.IdHocKiNavigation)
                .AsQueryable();

            int? namBatDau = null;
            int? namKetThuc = null;
            if (!string.IsNullOrWhiteSpace(namHoc))
            {
                var parts = namHoc.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out var start) && int.TryParse(parts[1], out var end))
                {
                    namBatDau = start;
                    namKetThuc = end;
                }
            }

            if (khoaId.HasValue && khoaId.Value > 0)
            {
                query = query.Where(d => d.IdKhoaHoc == khoaId);
            }

            if (namBatDau.HasValue && namKetThuc.HasValue)
            {
                query = query.Where(d => d.IdHocKiNavigation != null &&
                    d.IdHocKiNavigation.NamBatDau == namBatDau.Value &&
                    d.IdHocKiNavigation.NamKetThuc == namKetThuc.Value);
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
