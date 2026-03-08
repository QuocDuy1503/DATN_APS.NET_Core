namespace DATN_TMS.Areas.SinhVien.Models
{
    public class NopBaoCaoIndexViewModel
    {
        public string? TenDot { get; set; }
        public string? HocKi { get; set; }
        public NopBaoCaoBoxItem DeCuong { get; set; } = new();
        public NopBaoCaoBoxItem GiuaKy { get; set; } = new();
        public NopBaoCaoBoxItem CuoiKy { get; set; } = new();
    }

    public class NopBaoCaoBoxItem
    {
        public int? IdBaoCaoNop { get; set; }
        public string LoaiBaoCao { get; set; } = "";
        public string TieuDe { get; set; } = "";
        public string? ThoiGianBatDau { get; set; }
        public string? ThoiGianKetThuc { get; set; }
        public string? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TrangThaiCss { get; set; }
        public bool DangMo { get; set; }
    }

    public class NopBaoCaoDetailViewModel
    {
        public int? IdBaoCaoNop { get; set; }
        public string LoaiBaoCao { get; set; } = "";
        public string TieuDe { get; set; } = "";
        public string? TenDot { get; set; }

        public string? TrangThaiGui { get; set; }
        public string? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TrangThaiCss { get; set; }

        public string? TenFile { get; set; }
        public string? FilePath { get; set; }
        public string? NgaySuaDoiCuoi { get; set; }

        public string? GhiChuGui { get; set; }
        public string? NhanXetGVHD { get; set; }

        public string? ThoiGianBatDau { get; set; }
        public string? ThoiGianKetThuc { get; set; }
        public bool DangMo { get; set; }

        public int? IdDot { get; set; }
        public int? IdDeTai { get; set; }
        public int? IdSinhVien { get; set; }
    }
}
