using System;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace DATN_TMS.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public ThongBaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? filter, int? page)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            filter ??= "all";
            int pageNumber = page ?? 1;
            int pageSize = 10;

            var query = _context.ThongBaos
                .Where(tb => tb.IdNguoiNhan == userId);

            if (filter == "unread")
            {
                query = query.Where(tb => tb.TrangThaiXem != true);
            }
            else if (filter == "read")
            {
                query = query.Where(tb => tb.TrangThaiXem == true);
            }

            var pagedList = query
                .OrderByDescending(tb => tb.NgayTao ?? DateTime.MinValue)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.Filter = filter;
            return View(pagedList);
        }

        [HttpGet]
        public async Task<IActionResult> Latest()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, unreadCount = 0, items = Array.Empty<object>() });
            }

            var unreadCount = await _context.ThongBaos
                .Where(tb => tb.IdNguoiNhan == userId && (tb.TrangThaiXem == null || tb.TrangThaiXem == false))
                .CountAsync();

            var items = await _context.ThongBaos
                .Where(tb => tb.IdNguoiNhan == userId)
                .OrderByDescending(tb => tb.NgayTao ?? DateTime.MinValue)
                .Take(8)
                .Select(tb => new
                {
                    tb.Id,
                    tb.TieuDe,
                    NoiDung = tb.NoiDung ?? string.Empty,
                    Link = tb.LinkLienKet,
                    TrangThaiXem = tb.TrangThaiXem ?? false,
                    NgayTao = tb.NgayTao
                })
                .ToListAsync();

            return Json(new { success = true, unreadCount, items });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkNotificationRequest? request)
        {
            if (request == null || request.Id <= 0)
            {
                return Json(new { success = false, message = "D? li?u không h?p l?" });
            }

            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng ??ng nh?p l?i." });
            }

            var notif = await _context.ThongBaos
                .FirstOrDefaultAsync(tb => tb.Id == request.Id && tb.IdNguoiNhan == userId);

            if (notif == null)
            {
                return Json(new { success = false, message = "Không tìm th?y thông báo" });
            }

            if (notif.TrangThaiXem != true)
            {
                notif.TrangThaiXem = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng ??ng nh?p l?i." });
            }

            var notifs = await _context.ThongBaos
                .Where(tb => tb.IdNguoiNhan == userId && (tb.TrangThaiXem == null || tb.TrangThaiXem == false))
                .ToListAsync();

            if (notifs.Any())
            {
                foreach (var item in notifs)
                {
                    item.TrangThaiXem = true;
                }
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Open(int id)
        {
            var userId = await GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var notif = await _context.ThongBaos
                .FirstOrDefaultAsync(tb => tb.Id == id && tb.IdNguoiNhan == userId);

            if (notif == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (notif.TrangThaiXem != true)
            {
                notif.TrangThaiXem = true;
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrWhiteSpace(notif.LinkLienKet))
            {
                return Redirect(notif.LinkLienKet);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<int?> GetCurrentUserId()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            return user?.Id;
        }
    }

    public class MarkNotificationRequest
    {
        public int Id { get; set; }
    }
}
