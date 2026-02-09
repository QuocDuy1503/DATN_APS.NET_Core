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
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t báo cáo gi?a k?", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi ??t báo cáo gi?a k? b?t ??u", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_nop", Title = "Nh?c nh? n?p tài li?u: g?i sau X ngày khi báo cáo gi?a k? b?t ??u", MocThoiGian = "AFTER_START_NOP", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c nh? duy?t: g?i tr??c X ngày khi k?t thúc ??t báo cáo gi?a k?", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["baocao_cuoiki"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi m? ??t báo cáo cu?i k?", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo t? ??ng khi ??t ???c m?." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi ??t báo cáo cu?i k? b?t ??u", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_nop", Title = "Nh?c nh? n?p tài li?u: g?i sau X ngày khi báo cáo cu?i k? b?t ??u", MocThoiGian = "AFTER_START_NOP", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nh?c nh? duy?t: g?i tr??c X ngày khi k?t thúc ??t báo cáo cu?i k?", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_end_publish", Title = "Công b? ?i?m: g?i sau X ngày khi k?t thúc ??t báo cáo cu?i k?", MocThoiGian = "AFTER_END_PUBLISH", SoNgayChenhLech = 2, TrangThai = true }
            }
        };

        private static readonly List<ThongBaoCauHinhItem> _defaultTemplates = BuildDefaultTemplates();

        private static string GetDefaultTimeTitle(string loaiSuKien, string key)
        {
            return (loaiSuKien, key) switch
            {
                ("dangky_nguyenvong", "open") => "Khi m? ??t g?i thông báo",
                ("dangky_nguyenvong", "before_start") => "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u",
                ("dangky_nguyenvong", "before_end") => "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc",

                ("duyet_nguyenvong", "open") => "Khi m? ??t g?i thông báo",
                ("duyet_nguyenvong", "before_start") => "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u",
                ("duyet_nguyenvong", "before_end") => "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc",

                ("dexuat_detai", "open") => "Khi m? ??t g?i thông báo",
                ("dexuat_detai", "before_start") => "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u",
                ("dexuat_detai", "before_end") => "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc",

                ("duyet_detai", "open") => "Khi m? ??t g?i thông báo",
                ("duyet_detai", "after_start_hd") => "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi duy?t ?? tài b?t ??u",
                ("duyet_detai", "after_start_review") => "Nh?c nh? duy?t ?? tài: g?i sau X ngày khi duy?t ?? tài b?t ??u",
                ("duyet_detai", "before_end") => "Nh?c s?p k?t thúc duy?t ?? tài: g?i tr??c X ngày khi duy?t ?? tài k?t thúc",

                ("nop_decuong", "open") => "Khi m? ??t g?i thông báo",
                ("nop_decuong", "before_start") => "Nh?c m? ??t: g?i tr??c X ngày khi ??t b?t ??u",
                ("nop_decuong", "before_end") => "Nh?c s?p h?t h?n: g?i tr??c X ngày khi ??t k?t thúc",

                ("baocao_giuaki", "open") => "Khi m? ??t báo cáo gi?a k?",
                ("baocao_giuaki", "after_start_hd") => "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi ??t báo cáo gi?a k? b?t ??u",
                ("baocao_giuaki", "after_start_nop") => "Nh?c nh? n?p tài li?u: g?i sau X ngày khi báo cáo gi?a k? b?t ??u",
                ("baocao_giuaki", "before_end") => "Nh?c nh? duy?t: g?i tr??c X ngày khi k?t thúc ??t báo cáo gi?a k?",

                ("baocao_cuoiki", "open") => "Khi m? ??t báo cáo cu?i k?",
                ("baocao_cuoiki", "after_start_hd") => "Nh?c nh? l?p h?i ??ng: g?i sau X ngày khi ??t báo cáo cu?i k? b?t ??u",
                ("baocao_cuoiki", "after_start_nop") => "Nh?c nh? n?p tài li?u: g?i sau X ngày khi báo cáo cu?i k? b?t ??u",
                ("baocao_cuoiki", "before_end") => "Nh?c nh? duy?t: g?i tr??c X ngày khi k?t thúc ??t báo cáo cu?i k?",
                ("baocao_cuoiki", "after_end_publish") => "Công b? ?i?m: g?i sau X ngày khi k?t thúc ??t báo cáo cu?i k?",
                _ => string.Empty
            };
        }

        private static string? GetDefaultTimeNote(string loaiSuKien, string key)
        {
            return (loaiSuKien, key) switch
            {
                ("dangky_nguyenvong", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                ("duyet_nguyenvong", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                ("dexuat_detai", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                ("duyet_detai", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                ("nop_decuong", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                ("baocao_giuaki", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                ("baocao_cuoiki", "open") => "Thông báo t? ??ng khi ??t ???c m?.",
                _ => null
            };
        }

        private static bool LooksCorrupted(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            // Các ký t? th??ng xu?t hi?n khi l?i mã hóa: '?' ho?c '?' (U+FFFD)
            return value.Contains('?') || value.Contains('?');
        }

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
                    TieuDeMau = "[VLU] Nh?c báo cáo gi?a k?",
                    NoiDungMau = "??t {Ten_Dot} s?p ??n h?n báo cáo gi?a k? vào {Ngay_Bat_Dau}. Vui lòng chu?n b? h?i ??ng và tài li?u.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["baocao_giuaki"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "baocao_cuoiki",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nh?c báo cáo cu?i k?",
                    NoiDungMau = "??t {Ten_Dot} s?p ??n h?n báo cáo cu?i k? vào {Ngay_Bat_Dau}. Vui lòng chu?n b? h?i ??ng và tài li?u.",
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
                            var defaultTitle = GetDefaultTimeTitle(template.LoaiSuKien, t.Key);
                            var defaultNote = GetDefaultTimeNote(template.LoaiSuKien, t.Key);
                            return new ThongBaoThoiGianItem
                            {
                                Key = t.Key,
                                Title = string.IsNullOrWhiteSpace(defaultTitle) ? t.Title : defaultTitle,
                                MocThoiGian = t.MocThoiGian,
                                SoNgayChenhLech = matched?.SoNgayChenhLech ?? t.SoNgayChenhLech,
                                TrangThai = matched?.TrangThai ?? t.TrangThai,
                                ChoPhepNhapSoNgay = t.ChoPhepNhapSoNgay,
                                GhiChu = string.IsNullOrWhiteSpace(defaultNote) ? t.GhiChu : defaultNote
                            };
                        })
                        .ToList();

                    var first = currentGroup.FirstOrDefault();
                    return new ThongBaoCauHinhItem
                    {
                        LoaiSuKien = template.LoaiSuKien,
                        DoiTuongNhan = LooksCorrupted(first?.DoiTuongNhan) ? template.DoiTuongNhan : first?.DoiTuongNhan ?? template.DoiTuongNhan,
                        MocThoiGian = template.MocThoiGian,
                        SoNgayChenhLech = template.SoNgayChenhLech,
                        TrangThai = thoiGians.Any(x => x.TrangThai) || (first?.TrangThai ?? template.TrangThai),
                        TieuDeMau = LooksCorrupted(first?.TieuDeMau) ? template.TieuDeMau : first?.TieuDeMau ?? template.TieuDeMau,
                        NoiDungMau = LooksCorrupted(first?.NoiDungMau) ? template.NoiDungMau : first?.NoiDungMau ?? template.NoiDungMau,
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

                var defaultTemplate = _defaultTemplates.FirstOrDefault(x => x.LoaiSuKien == item.LoaiSuKien);
                if (defaultTemplate != null)
                {
                    if (LooksCorrupted(item.TieuDeMau))
                    {
                        item.TieuDeMau = defaultTemplate.TieuDeMau;
                    }

                    if (LooksCorrupted(item.NoiDungMau))
                    {
                        item.NoiDungMau = defaultTemplate.NoiDungMau;
                    }

                    if (LooksCorrupted(item.DoiTuongNhan))
                    {
                        item.DoiTuongNhan = defaultTemplate.DoiTuongNhan;
                    }
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
