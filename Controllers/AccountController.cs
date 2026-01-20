using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Http;

namespace DATN_TMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        public AccountController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserEmail") != null)
            {
                if (HttpContext.Session.GetString("Role") == "BCN_KHOA")
                {
                    return RedirectToAction("Index", "QuanLyDotDoAn", new { area = "BCNKhoa" });
                }
                return RedirectToAction("Index", "QuanLyDotDoAn");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.NguoiDungs
                .Include(u => u.IdVaiTros)
                .FirstOrDefaultAsync(u => u.Email == email && u.MatKhau == password && u.TrangThai == 1);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
                return View();
            }

            var roleInfo = user.IdVaiTros.FirstOrDefault();
            string roleCode = roleInfo?.MaVaiTro ?? "GUEST";

            string userCode = "";

            if (roleCode == "SV")
            {
                var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.IdNguoiDung == user.Id);

                if (sv != null)
                {
                    userCode = sv.Mssv!;
                }
            }
            else
            {
                var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.IdNguoiDung == user.Id);
                if (gv != null)
                {
                    userCode = gv.MaGv ?? "GV";
                }
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("FullName", user.HoTen ?? "Người dùng");
            HttpContext.Session.SetString("Role", roleCode);
            HttpContext.Session.SetString("UserCode", userCode);

            if (roleCode == "BCN_KHOA")
            {
                return RedirectToAction("Index", "QuanLyDotDoAn", new { area = "BCNKhoa" });
            }

            else if (roleCode == "BO_MON")
            {
                return RedirectToAction("Index", "QuanLyDotDoAn");
            }

            else if (roleCode == "GV")
            {
                return RedirectToAction("Index", "QLSV");
            }

            else if (roleCode == "SV")
            {
                return RedirectToAction("Index", "QuanLyDotDoAn");
            }

            return RedirectToAction("Index", "QuanLyDotDoAn");

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}