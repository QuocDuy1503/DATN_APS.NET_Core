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

        private static readonly Dictionary<string, List<ThongBaoThoiGianItem>> _defaultTimeConfigs = new()
        {
            ["dangky_nguyenvong"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t g?i thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["duyet_nguyenvong"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t g?i thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["dexuat_detai"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t g?i thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["duyet_detai"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t g?i thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi duy?t ?? tài b?t ??u", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_review", Title = "Nh?c nh? duy?t ?? tài: g?i sau X ngày khi duy?t ?? tài b?t ??u", MocThoiGian = "AFTER_START_REVIEW", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c s?p k?t thúc duy?t ?? tài: g?i tr??c X ngày khi duy?t ?? tài k?t thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["nop_decuong"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t g?i thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["baocao_giuaki"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t báo cáo gi?a kì", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi ??t báo cáo gi?a kì b?t ??u", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_nop", Title = "Nh?c nh? n?p tài li?u: g?i sau X ngày khi báo cáo gi?a kì b?t ??u", MocThoiGian = "AFTER_START_NOP", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c nh? duy?t: g?i tr??c X ngày khi k?t thúc ??t báo cáo gi?a kì", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["baocao_cuoiki"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t báo cáo cu?i kì", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi ??t báo cáo cu?i kì b?t ??u", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_nop", Title = "Nh?c nh? n?p tài li?u: g?i sau X ngày khi báo cáo cu?i kì b?t ??u", MocThoiGian = "AFTER_START_NOP", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c nh? duy?t: g?i tr??c X ngày khi k?t thúc ??t báo cáo cu?i kì", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_end_publish", Title = "Công b? ?i?m: g?i sau X ngày khi k?t thúc ??t báo cáo cu?i kì", MocThoiGian = "AFTER_END_PUBLISH", SoNgayChenhLech = 2, TrangThai = true }
            }
        };

        private static readonly List<ThongBaoCauHinhItem> _defaultTemplates = BuildDefaultTemplates();

        private static List<ThongBaoCauHinhItem> BuildDefaultTemplates()
        {
            var list = new List<ThongBaoCauHinhItem>
            {
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "dangky_nguyenvong",
                    DoiTuongNhan = "sinhvien",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c ??ng ký nguy?n v?ng",
                    NoiDungMau = "??t {Ten_Dot} s?p m? ??ng ký nguy?n v?ng vào {Ngay_Bat_Dau}. Vui lòng chu?n b? và ??ng ký ?úng h?n.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["dangky_nguyenvong"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "duyet_nguyenvong",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c duy?t ??ng ký nguy?n v?ng",
                    NoiDungMau = "??t {Ten_Dot} s?p h?t h?n duy?t ??ng ký nguy?n v?ng vào {Ngay_Ket_Thuc}. Vui lòng hoàn t?t duy?t.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["duyet_nguyenvong"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "dexuat_detai",
                    DoiTuongNhan = "sinhvien,giangvien",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c ?? xu?t ?? tài",
                    NoiDungMau = "??t {Ten_Dot} s?p h?t h?n ?? xu?t ?? tài vào {Ngay_Ket_Thuc}. Vui lòng hoàn t?t tr??c h?n.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["dexuat_detai"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "duyet_detai",
                    DoiTuongNhan = "hoidong",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Phân công duy?t ?? tài",
                    NoiDungMau = "H?i ??ng ???c phân công duy?t ?? tài thu?c ??t {Ten_Dot}. Vui lòng truy c?p h? th?ng ?? xem danh sách và th?i h?n.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["duyet_detai"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "nop_decuong",
                    DoiTuongNhan = "sinhvien",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c n?p ?? c??ng",
                    NoiDungMau = "??t {Ten_Dot} s?p h?t h?n n?p ?? c??ng vào {Ngay_Ket_Thuc}. Vui lòng hoàn thành ?úng h?n.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["nop_decuong"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "baocao_giuaki",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c báo cáo gi?a kì",
                    NoiDungMau = "??t {Ten_Dot} s?p ??n h?n báo cáo gi?a kì vào {Ngay_Bat_Dau}. Vui lòng chu?n b? h?i ??ng và tài li?u.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["baocao_giuaki"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "baocao_cuoiki",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c báo cáo cu?i kì",
                    NoiDungMau = "??t {Ten_Dot} s?p ??n h?n báo cáo cu?i kì vào {Ngay_Bat_Dau}. Vui lòng chu?n b? h?i ??ng và tài li?u.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["baocao_cuoiki"])
                }
            };

            foreach (var item in list)
            {
                var firstTime = item.ThoiGians.FirstOrDefault();
                if (firstTime != null)
                {
                    item.MocThoiGian = firstTime.MocThoiGian;
                    item.SoNgayChenhLech = firstTime.SoNgayChenhLech;
                    item.TrangThai = item.ThoiGians.Any(x => x.TrangThai);
                }
            }

            return list;
        }

        private static List<ThongBaoThoiGianItem> CloneTimeConfigs(List<ThongBaoThoiGianItem> source)
        {
            return source
                .Select(x => new ThongBaoThoiGianItem
                {
                    Key = x.Key,
                    Title = x.Title,
                    MocThoiGian = x.MocThoiGian,
                    SoNgayChenhLech = x.SoNgayChenhLech,
                    TrangThai = x.TrangThai,
                    ChoPhepNhapSoNgay = x.ChoPhepNhapSoNgay,
                    GhiChu = x.GhiChu
                })
                .ToList();
        }

        public QuanLyThongBaoController(QuanLyDoAnTotNghiepContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var existingConfigs = await _context.CauHinhThongBaos
                .Where(x => x.IdDot == null)
                .ToListAsync();

            var order = new List<string>
            {
                "dangky_nguyenvong",
                "duyet_nguyenvong",
                "dexuat_detai",
                "duyet_detai",
                "nop_decuong",
                "baocao_giuaki",
                "baocao_cuoiki"
            };

            var cauHinhs = _defaultTemplates
                .Select(template =>
                {
                    var currentGroup = existingConfigs
                        .Where(x => x.LoaiSuKien == template.LoaiSuKien)
                        .ToList();

                    var thoiGians = template.ThoiGians
                        .Select(t =>
                        {
                            var matched = currentGroup.FirstOrDefault(x => x.MocThoiGian == t.MocThoiGian);
                            return new ThongBaoThoiGianItem
                            {
                                Key = t.Key,
                                Title = t.Title,
                                MocThoiGian = t.MocThoiGian,
                                SoNgayChenhLech = matched?.SoNgayChenhLech ?? t.SoNgayChenhLech,
                                TrangThai = matched?.TrangThai ?? t.TrangThai,
                                ChoPhepNhapSoNgay = t.ChoPhepNhapSoNgay,
                                GhiChu = t.GhiChu
                            };
                        })
                        .ToList();

                    var first = currentGroup.FirstOrDefault();
                    return new ThongBaoCauHinhItem
                    {
                        LoaiSuKien = template.LoaiSuKien,
                        DoiTuongNhan = first?.DoiTuongNhan ?? template.DoiTuongNhan,
                        MocThoiGian = template.MocThoiGian,
                        SoNgayChenhLech = template.SoNgayChenhLech,
                        TrangThai = thoiGians.Any(x => x.TrangThai) || (first?.TrangThai ?? template.TrangThai),
                        TieuDeMau = first?.TieuDeMau ?? template.TieuDeMau,
                        NoiDungMau = first?.NoiDungMau ?? template.NoiDungMau,
                        ThoiGians = thoiGians
                    };
                })
                .OrderBy(x => order.IndexOf(x.LoaiSuKien))
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

                var timeConfigs = item.ThoiGians ?? new List<ThongBaoThoiGianItem>();

                if (!timeConfigs.Any())
                {
                    timeConfigs.Add(new ThongBaoThoiGianItem
                    {
                        Key = "default",
                        MocThoiGian = item.MocThoiGian ?? string.Empty,
                        SoNgayChenhLech = item.SoNgayChenhLech,
                        TrangThai = item.TrangThai,
                        ChoPhepNhapSoNgay = true
                    });
                }

                foreach (var tg in timeConfigs)
                {
                    if (string.IsNullOrWhiteSpace(tg.MocThoiGian))
                    {
                        continue;
                    }

                    var entity = existing.FirstOrDefault(x => x.LoaiSuKien == item.LoaiSuKien && x.MocThoiGian == tg.MocThoiGian);
                    if (entity == null)
                    {
                        entity = new CauHinhThongBao
                        {
                            IdDot = null,
                            LoaiSuKien = item.LoaiSuKien,
                            MocThoiGian = tg.MocThoiGian
                        };
                        _context.CauHinhThongBaos.Add(entity);
                        existing.Add(entity);
                    }

                    entity.DoiTuongNhan = item.DoiTuongNhan;
                    entity.MocThoiGian = tg.MocThoiGian;
                    entity.SoNgayChenhLech = tg.SoNgayChenhLech;
                    entity.TrangThai = tg.TrangThai;
                    entity.TieuDeMau = item.TieuDeMau;
                    entity.NoiDungMau = item.NoiDungMau;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "?ã l?u c?u hình thông báo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
