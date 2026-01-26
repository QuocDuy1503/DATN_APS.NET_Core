namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class CTDTViewModel
    {
        public int? KhoaHocId { get; set; }
        public string? SearchString { get; set; }
        public List<CTDTItem> ChuongTrinhs { get; set; } = new();
        public List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>? KhoaOptions { get; set; }
    }

    public class CTDTItem
    {
        public int Id { get; set; }
        public string MaCtdt { get; set; } = string.Empty;
        public string TenCtdt { get; set; } = string.Empty;
        public string Nganh { get; set; } = string.Empty;
        public string Khoa { get; set; } = string.Empty;
        public int TongTinChi { get; set; }
        public bool TrangThai { get; set; }
    }

    public class CTDTDetailViewModel
    {
        public int Id { get; set; }
        public string MaCtdt { get; set; } = string.Empty;
        public string TenCtdt { get; set; } = string.Empty;
        public string Khoa { get; set; } = string.Empty;
        public string Nganh { get; set; }
        public int TongTinChi { get; set; }
        public List<CTDTHocPhanViewModel> HocPhans { get; set; } = new();
    }

    public class CTDTHocPhanViewModel
    {
        public int Stt { get; set; }
        public string MaHocPhan { get; set; } = string.Empty;
        public string TenHocPhan { get; set; } = string.Empty;
        public int SoTinChi { get; set; }
        public string LoaiHocPhan { get; set; } = string.Empty;
        public string? DieuKienTienQuyet { get; set; }
        public int HocKiToChuc { get; set; }
    }
}
