namespace DATN_TMS.Areas.SinhVien.Models
{
    public class KetQuaBaoCaoViewModel
    {
        public string? MaSinhVien { get; set; }
        public string? HoTen { get; set; }
        public string? TenDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenGVHD { get; set; }
        public string? TenDot { get; set; }
        public KetQuaHoiDongItem? KetQuaGiuaKy { get; set; }
        public KetQuaHoiDongItem? KetQuaCuoiKy { get; set; }
    }

    public class KetQuaHoiDongItem
    {
        public string? TenHoiDong { get; set; }
        public string? LoaiHoiDong { get; set; }
        public string? DiaDiem { get; set; }
        public string? NgayBaoCao { get; set; }
        public string? ThoiGian { get; set; }
        public double? DiemTongKet { get; set; }
        public string? DiemChu { get; set; }
        public string? KetQua { get; set; }
        public List<DiemTieuChiItem> DanhSachDiemTieuChi { get; set; } = new();
        public List<ThanhVienHoiDongItem> DanhSachThanhVien { get; set; } = new();
    }

    public class DiemTieuChiItem
    {
        public int Stt { get; set; }
        public string? TenTieuChi { get; set; }
        public double? TrongSo { get; set; }
        public double? DiemToiDa { get; set; }
        public double? DiemSo { get; set; }
        public string? NhanXet { get; set; }
        public string? TenNguoiCham { get; set; }
    }

    public class ThanhVienHoiDongItem
    {
        public string? HoTen { get; set; }
        public string? VaiTro { get; set; }
    }
}
