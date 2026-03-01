using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

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
            var role = HttpContext.User?.FindFirstValue(ClaimTypes.Role) ?? HttpContext.Session.GetString("Role");

            if (HttpContext.User?.Identity?.IsAuthenticated == true || !string.IsNullOrEmpty(role))
            {
                if (role == "BCN_KHOA")
                {
                    return RedirectToAction("Index", "QuanLyDotDoAn", new { area = "BCNKhoa" });
                }

                if (role == "BO_MON")
                {
                    return RedirectToAction("Index", "QuanLyDangKy", new { area = "GV_BoMon" });
                }

                if (role == "GIANG_VIEN")
                {
                    return RedirectToAction("Index", "QuanLyDotDoAn", new { area = "GiangVien" });
                }

                if (role == "SINH_VIEN" || role == "SV")
                {
                    return RedirectToAction("Index", "DangKyNguyenVong", new { area = "SinhVien" });
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
                .FirstOrDefaultAsync(u => u.Email == email && u.TrangThai == 1);

            if (user == null || !VerifyPassword(password, user.MatKhau))
            {
                ViewBag.Error = "Tài khoản hoặc mật khẩu không chính xác!";
                return View();
            }
            // Ưu tiên role Sinh viên nếu có
            var isSinhVien = user.IdVaiTros.Any(v => v.MaVaiTro == "SINH_VIEN" || v.MaVaiTro == "SV");
            var sv = await _context.SinhViens.FirstOrDefaultAsync(s => s.IdNguoiDung == user.Id);
            if (sv != null) isSinhVien = true;

            string roleCode;

            if (isSinhVien)
            {
                roleCode = "SINH_VIEN";
            }
            else
            {
                var priority = new[] { "BCN_KHOA", "BO_MON", "GIANG_VIEN" };
                roleCode = user.IdVaiTros
                    .Where(v => priority.Contains(v.MaVaiTro))
                    .OrderBy(v => Array.IndexOf(priority, v.MaVaiTro))
                    .Select(v => v.MaVaiTro)
                    .FirstOrDefault() ?? "GUEST";
            }

            string userCode = "";

            if (roleCode == "SINH_VIEN" && sv != null)
            {
                userCode = sv.Mssv ?? "SV";
            }
            else
            {
                var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.IdNguoiDung == user.Id);
                if (gv != null)
                {
                    userCode = gv.MaGv ?? "GV";
                }
            }

            // Claims + Cookie auth
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleCode)
            };

            if (roleCode == "SINH_VIEN")
            {
                // Thêm role ngắn để tương thích các authorize cũ dùng "SV"
                claims.Add(new Claim(ClaimTypes.Role, "SV"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            // Session cho các view/layout đang dùng
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
                return RedirectToAction("Index", "QuanLyDangKy", new { area = "GV_BoMon" });
            }
            else if (roleCode == "GIANG_VIEN")
            {
                return RedirectToAction("Index", "QuanLyDotDoAn", new { area = "GiangVien" });
            }
            else if (roleCode == "SINH_VIEN")
            {
                return RedirectToAction("Index", "DangKyNguyenVong", new { area = "SinhVien" });
            }

            return RedirectToAction("Index", "QuanLyDotDoAn");

        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private static bool VerifyPassword(string inputPassword, string? storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword)) return false;

            // Nếu DB lưu plain text (legacy)
            if (storedPassword == inputPassword) return true;

            // Thử kiểm tra dạng hash SHA256
            using var sha = SHA256.Create();
            var hashedInput = Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(inputPassword)));
            // stored có thể đang để lowercase/uppercase khác nhau
            return string.Equals(hashedInput, storedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}