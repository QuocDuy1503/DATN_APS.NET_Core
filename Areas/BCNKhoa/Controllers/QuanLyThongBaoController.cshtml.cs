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
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt gửi thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["duyet_nguyenvong"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt gửi thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["dexuat_detai"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt gửi thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["duyet_detai"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt gửi thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nhắc nhở lập hội đồng: gửi sau X ngày khi duyệt đề tài bắt đầu", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_review", Title = "Nhắc nhở duyệt đề tài: gửi sau X ngày khi duyệt đề tài bắt đầu", MocThoiGian = "AFTER_START_REVIEW", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc sắp kết thúc duyệt đề tài: gửi trước X ngày khi duyệt đề tài kết thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["nop_decuong"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt gửi thông báo", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "before_start", Title = "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu", MocThoiGian = "BEFORE_START", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["baocao_giuaki"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt báo cáo giữa kỳ", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nhắc nhở lập hội đồng: gửi sau X ngày khi đợt báo cáo giữa kỳ bắt đầu", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_nop", Title = "Nhắc nhở nộp tài liệu: gửi sau X ngày khi báo cáo giữa kỳ bắt đầu", MocThoiGian = "AFTER_START_NOP", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc nhở duyệt: gửi trước X ngày khi kết thúc đợt báo cáo giữa kỳ", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true }
            },
            ["baocao_cuoiki"] = new()
            {
                new ThongBaoThoiGianItem { Key = "open", Title = "Khi mở đợt báo cáo cuối kỳ", MocThoiGian = "ON_START", SoNgayChenhLech = 0, TrangThai = true, ChoPhepNhapSoNgay = false, GhiChu = "Thông báo tự động khi đợt được mở." },
                new ThongBaoThoiGianItem { Key = "after_start_hd", Title = "Nhắc nhở lập hội đồng: gửi sau X ngày khi đợt báo cáo cuối kỳ bắt đầu", MocThoiGian = "AFTER_START_HD", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_start_nop", Title = "Nhắc nhở nộp tài liệu: gửi sau X ngày khi báo cáo cuối kỳ bắt đầu", MocThoiGian = "AFTER_START_NOP", SoNgayChenhLech = 3, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "before_end", Title = "Nhắc nhở duyệt: gửi trước X ngày khi kết thúc đợt báo cáo cuối kỳ", MocThoiGian = "BEFORE_END", SoNgayChenhLech = 2, TrangThai = true },
                new ThongBaoThoiGianItem { Key = "after_end_publish", Title = "Công bố điểm: gửi sau X ngày khi kết thúc đợt báo cáo cuối kỳ", MocThoiGian = "AFTER_END_PUBLISH", SoNgayChenhLech = 2, TrangThai = true }
            }
        };

        private static readonly List<ThongBaoCauHinhItem> _defaultTemplates = BuildDefaultTemplates();

        private static string GetDefaultTimeTitle(string loaiSuKien, string key)
        {
            return (loaiSuKien, key) switch
            {
                ("dangky_nguyenvong", "open") => "Khi mở đợt gửi thông báo",
                ("dangky_nguyenvong", "before_start") => "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu",
                ("dangky_nguyenvong", "before_end") => "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc",

                ("duyet_nguyenvong", "open") => "Khi mở đợt gửi thông báo",
                ("duyet_nguyenvong", "before_start") => "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu",
                ("duyet_nguyenvong", "before_end") => "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc",

                ("dexuat_detai", "open") => "Khi mở đợt gửi thông báo",
                ("dexuat_detai", "before_start") => "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu",
                ("dexuat_detai", "before_end") => "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc",

                ("duyet_detai", "open") => "Khi mở đợt gửi thông báo",
                ("duyet_detai", "after_start_hd") => "Nhắc nhở lập hội đồng: gửi sau X ngày khi duyệt đề tài bắt đầu",
                ("duyet_detai", "after_start_review") => "Nhắc nhở duyệt đề tài: gửi sau X ngày khi duyệt đề tài bắt đầu",
                ("duyet_detai", "before_end") => "Nhắc sắp kết thúc duyệt đề tài: gửi trước X ngày khi duyệt đề tài kết thúc",

                ("nop_decuong", "open") => "Khi mở đợt gửi thông báo",
                ("nop_decuong", "before_start") => "Nhắc mở đợt: gửi trước X ngày khi đợt bắt đầu",
                ("nop_decuong", "before_end") => "Nhắc sắp hết hạn: gửi trước X ngày khi đợt kết thúc",

                ("baocao_giuaki", "open") => "Khi mở đợt báo cáo giữa kỳ",
                ("baocao_giuaki", "after_start_hd") => "Nhắc nhở lập hội đồng: gửi sau X ngày khi đợt báo cáo giữa kỳ bắt đầu",
                ("baocao_giuaki", "after_start_nop") => "Nhắc nhở nộp tài liệu: gửi sau X ngày khi báo cáo giữa kỳ bắt đầu",
                ("baocao_giuaki", "before_end") => "Nhắc nhở duyệt: gửi trước X ngày khi kết thúc đợt báo cáo giữa kỳ",

                ("baocao_cuoiki", "open") => "Khi mở đợt báo cáo cuối kỳ",
                ("baocao_cuoiki", "after_start_hd") => "Nhắc nhở lập hội đồng: gửi sau X ngày khi đợt báo cáo cuối kỳ bắt đầu",
                ("baocao_cuoiki", "after_start_nop") => "Nhắc nhở nộp tài liệu: gửi sau X ngày khi báo cáo cuối kỳ bắt đầu",
                ("baocao_cuoiki", "before_end") => "Nhắc nhở duyệt: gửi trước X ngày khi kết thúc đợt báo cáo cuối kỳ",
                ("baocao_cuoiki", "after_end_publish") => "Công bố điểm: gửi sau X ngày khi kết thúc đợt báo cáo cuối kỳ",
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

            // Các ký tự thường xuất hiện khi lỗi mã hóa: '?' hoặc '�' (U+FFFD)
            return value.Contains('?') || value.Contains('�');
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
                    TieuDeMau = "[VLU] Nhắc đăng ký nguyện vọng",
                    NoiDungMau = "Đợt {Ten_Dot} sắp mở đăng ký nguyện vọng vào {Ngay_Bat_Dau}. Vui lòng chuẩn bị và đăng ký đúng hạn.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["dangky_nguyenvong"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "duyet_nguyenvong",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nhắc duyệt đăng ký nguyện vọng",
                    NoiDungMau = "Đợt {Ten_Dot} sắp hết hạn duyệt đăng ký nguyện vọng vào {Ngay_Ket_Thuc}. Vui lòng hoàn tất duyệt.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["duyet_nguyenvong"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "dexuat_detai",
                    DoiTuongNhan = "sinhvien,giangvien",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nhắc đề xuất đề tài",
                    NoiDungMau = "Đợt {Ten_Dot} sắp hết hạn đề xuất đề tài vào {Ngay_Ket_Thuc}. Vui lòng hoàn tất trước hạn.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["dexuat_detai"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "duyet_detai",
                    DoiTuongNhan = "hoidong",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Phân công duyệt đề tài",
                    NoiDungMau = "Hội đồng được phân công duyệt đề tài thuộc đợt {Ten_Dot}. Vui lòng truy cập hệ thống để xem danh sách và thời hạn.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["duyet_detai"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "nop_decuong",
                    DoiTuongNhan = "sinhvien",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nhắc nộp đề cương",
                    NoiDungMau = "Đợt {Ten_Dot} sắp hết hạn nộp đề cương vào {Ngay_Ket_Thuc}. Vui lòng hoàn thành đúng hạn.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["nop_decuong"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "baocao_giuaki",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nhắc báo cáo giữa kỳ",
                    NoiDungMau = "Đợt {Ten_Dot} sắp đến hạn báo cáo giữa kỳ vào {Ngay_Bat_Dau}. Vui lòng chuẩn bị hội đồng và tài liệu.",
                    ThoiGians = CloneTimeConfigs(_defaultTimeConfigs["baocao_giuaki"])
                },
                new ThongBaoCauHinhItem
                {
                    LoaiSuKien = "baocao_cuoiki",
                    DoiTuongNhan = "bomon,bcnkhoa",
                    TrangThai = true,
                    TieuDeMau = "[VLU] Nhắc báo cáo cuối kỳ",
                    NoiDungMau = "Đợt {Ten_Dot} sắp đến hạn báo cáo cuối kỳ vào {Ngay_Bat_Dau}. Vui lòng chuẩn bị hội đồng và tài liệu.",
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
            TempData["SuccessMessage"] = "Đã lưu cấu hình thông báo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
