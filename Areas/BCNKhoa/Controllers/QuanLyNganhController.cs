using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using DATN_TMS.Areas.BCNKhoa.Models; // Nhớ using namespace chứa ViewModel
using X.PagedList;
using X.PagedList.Extensions; // Để dùng .ToPagedList()
using System.Linq;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyNganhController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public QuanLyNganhController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            ViewBag.CurrentFilter = searchString;

            // Truy vấn cơ bản
            var query = _context.Nganhs.AsQueryable();

            // Lọc dữ liệu
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(n => n.MaNganh.Contains(searchString) ||
                                         n.TenNganh.Contains(searchString));
            }

            // Sắp xếp
            query = query.OrderByDescending(n => n.Id);

            // SELECT: Chuyển đổi sang ViewModel với đúng các cột yêu cầu
            var modelQuery = query.Select(n => new NganhViewModel
            {
                Id = n.Id,
                MaNganh = n.MaNganh,
                TenNganh = n.TenNganh ?? "",

        
                TenBoMon = n.IdBoMonNavigation != null ? n.IdBoMonNavigation.TenBoMon : "",


                NguoiTao = n.IdNguoiTaoNavigation != null ? n.IdNguoiTaoNavigation.HoTen : "",

                NgayTao = n.NgayTao,

                // Lấy tên người sửa (Không cần ép kiểu (NguoiDung) nữa)
                NguoiSua = n.IdNguoiSuaNavigation != null ? n.IdNguoiSuaNavigation.HoTen : "",

                NgaySua = n.NgaySua
            });

       
            var pagedList = modelQuery.ToPagedList(pageNumber, pageSize);

            return View(pagedList);
        }
    }
}