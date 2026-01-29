using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class ThongBaoViewModel
    {
        public int? SelectedDotId { get; set; }

        public List<SelectListItem> DotOptions { get; set; } = new();

        public List<ThongBaoCauHinhItem> CauHinhs { get; set; } = new();
    }

    public class ThongBaoCauHinhItem
    {
        public string LoaiSuKien { get; set; } = string.Empty;

        public string? DoiTuongNhan { get; set; }

        public string? MocThoiGian { get; set; }

        public int? SoNgayChenhLech { get; set; }

        public bool TrangThai { get; set; }

        public string? TieuDeMau { get; set; }

        public string? NoiDungMau { get; set; }
    }
}
