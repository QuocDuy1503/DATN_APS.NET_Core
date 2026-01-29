using DATN_TMS.Areas.BCNKhoa.Models;
using DATN_TMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN_TMS.Areas.BCNKhoa.Controllers
{
    [Area("BCNKhoa")]
    public class QuanLyThongBaoController : Controller
    {
        private readonly QuanLyDoAnTotNghiepContext _context;

        private static readonly List<ThongBaoCauHinhItem> _defaultTemplates = new()
        {
            new ThongBaoCauHinhItem
            {
                LoaiSuKien = "dangky_detai",
                DoiTuongNhan = "sinhvien",
                MocThoiGian = "ON_START",
                SoNgayChenhLech = 0,
                TrangThai = true,
                TieuDeMau = "[VLU] Xác nh?n ??ng ký ?? tài",
                NoiDungMau = "Xin chào {Ten_Sinh_Vien}, b?n ?ã ??ng ký ?? tài {Ten_De_Tai} trong ??t {Ten_Dot}. Vui lòng theo dõi ti?n ?? và hoàn thành các m?c ti?p theo."
            },
            new ThongBaoCauHinhItem
            {
                LoaiSuKien = "dexuat_detai",
                DoiTuongNhan = "sinhvien,giangvien",
                MocThoiGian = "BEFORE_END",
                SoNgayChenhLech = 2,
                TrangThai = true,
                TieuDeMau = "[VLU] Nh?c nh? ?? xu?t ?? tài",
                NoiDungMau = "??t {Ten_Dot} s?p h?t h?n ?? xu?t ?? tài vào {Ngay_Ket_Thuc}. Vui lòng hoàn t?t tr??c h?n."
            },
            new ThongBaoCauHinhItem
            {
                LoaiSuKien = "duyet_detai",
                DoiTuongNhan = "hoidong",
                MocThoiGian = "ON_START",
                SoNgayChenhLech = 0,
                TrangThai = true,
                TieuDeMau = "[VLU] Phân công duy?t ?? tài",
                NoiDungMau = "H?i ??ng ???c phân công duy?t ?? tài thu?c ??t {Ten_Dot}. Vui lòng truy c?p h? th?ng ?? xem danh sách và th?i h?n."
            },
            new ThongBaoCauHinhItem
            {
                LoaiSuKien = "duyet_nguyenvong",
                DoiTuongNhan = "bomon",
                MocThoiGian = "BEFORE_END",
                SoNgayChenhLech = 1,
                TrangThai = true,
                TieuDeMau = "[VLU] Nh?c duy?t ??ng ký nguy?n v?ng",
                NoiDungMau = "??t {Ten_Dot} s?p h?t h?n duy?t ??ng ký nguy?n v?ng vào {Ngay_Ket_Thuc}. Vui lòng hoàn t?t duy?t."
            },
            new ThongBaoCauHinhItem
            {
                LoaiSuKien = "ketqua_cuoiki",
                DoiTuongNhan = "sinhvien",
                MocThoiGian = "ON_RESULT",
                SoNgayChenhLech = 0,
                TrangThai = true,
                TieuDeMau = "[VLU] Công b? k?t qu? cu?i kì",
                NoiDungMau = "K?t qu? ?? án {Ten_Dot} ?ã ???c công b?. Vui lòng ??ng nh?p ?? xem chi ti?t."
            }
        };

        public QuanLyThongBaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var existingConfigs = await _context.CauHinhThongBaos
                .Where(x => x.IdDot == null)
                .ToListAsync();

            var cauHinhs = _defaultTemplates
                .Select(template =>
                {
                    var current = existingConfigs.FirstOrDefault(x => x.LoaiSuKien == template.LoaiSuKien);
                    return new ThongBaoCauHinhItem
                    {
                        LoaiSuKien = template.LoaiSuKien,
                        DoiTuongNhan = current?.DoiTuongNhan ?? template.DoiTuongNhan,
                        MocThoiGian = current?.MocThoiGian ?? template.MocThoiGian,
                        SoNgayChenhLech = current?.SoNgayChenhLech ?? template.SoNgayChenhLech,
                        TrangThai = current?.TrangThai ?? template.TrangThai,
                        TieuDeMau = current?.TieuDeMau ?? template.TieuDeMau,
                        NoiDungMau = current?.NoiDungMau ?? template.NoiDungMau
                    };
                })
                .ToList();

            var vm = new ThongBaoViewModel
            {
                SelectedDotId = null,
                DotOptions = new List<SelectListItem>(),
                CauHinhs = cauHinhs
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ThongBaoViewModel model)
        {
            var existing = await _context.CauHinhThongBaos
                .Where(x => x.IdDot == null)
                .ToListAsync();

            foreach (var item in model.CauHinhs ?? new List<ThongBaoCauHinhItem>())
            {
                if (string.IsNullOrWhiteSpace(item.LoaiSuKien))
                {
                    continue;
                }

                var entity = existing.FirstOrDefault(x => x.LoaiSuKien == item.LoaiSuKien);
                if (entity == null)
                {
                    entity = new CauHinhThongBao
                    {
                        IdDot = null,
                        LoaiSuKien = item.LoaiSuKien
                    };
                    _context.CauHinhThongBaos.Add(entity);
                }

                entity.DoiTuongNhan = item.DoiTuongNhan;
                entity.MocThoiGian = item.MocThoiGian;
                entity.SoNgayChenhLech = item.SoNgayChenhLech;
                entity.TrangThai = item.TrangThai;
                entity.TieuDeMau = item.TieuDeMau;
                entity.NoiDungMau = item.NoiDungMau;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "?ã l?u c?u hình thông báo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
