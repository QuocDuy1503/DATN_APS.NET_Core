using Microsoft.AspNetCore.Mvc;
using DATN_TMS.Models;
using DATN_TMS.Areas.BCNKhoa.Models; // Using ViewModel
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using System.Linq;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyBoMonController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyBoMonController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? page, string searchString)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            var query = _context.BoMons.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => b.TenBoMon.Contains(searchString)
                                      || b.TenVietTat.Contains(searchString));
            }

            query = query.OrderBy(b => b.Stt).ThenBy(b => b.TenBoMon);

            // Chuyển đổi sang ViewModel
            var modelQuery = query.Select(b => new BoMonViewModel
            {
                Id = b.Id,
                Stt = b.Stt,
                TenBoMon = b.TenBoMon ?? "",
                TenVietTat = b.TenVietTat ?? "",

                // Lấy tên người tạo/sửa an toàn
                NguoiTao = b.IdNguoiTaoNavigation != null ? b.IdNguoiTaoNavigation.HoTen : "",
                NgayTao = b.NgayTao,

                NguoiSua = b.IdNguoiSuaNavigation != null ? b.IdNguoiSuaNavigation.HoTen : "",
                NgaySua = b.NgaySua
            });

            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }
    }
}