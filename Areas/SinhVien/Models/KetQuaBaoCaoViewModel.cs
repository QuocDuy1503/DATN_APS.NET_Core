namespace DATN_TMS.Areas.SinhVien.Models
{
    public class KetQuaBaoCaoViewModel
    {
        public string? MaSinhVien { get; set; }
        public string? HoTen { get; set; }
        public string? TenDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenGVHD { get; set; }

        // Thông tin đợt
        public string? TenDot { get; set; }
        public string? HocKi { get; set; }
        public string? ThoiGianDot { get; set; }
        public bool CoDot { get; set; }
        public string? ThongBaoDot { get; set; }

        // Trạng thái
        public bool CoDeTai { get; set; }
        public string? TrangThaiDeTai { get; set; }

        // Điểm GVHD (30%)
        public double? DiemGVHD { get; set; }
        public string? NhanXetGVHD { get; set; }
        public bool CoDiemGVHD => DiemGVHD.HasValue;

        // Kết quả hội đồng bảo vệ (70%)
        public KetQuaHoiDongItem? KetQuaGiuaKy { get; set; }
        public KetQuaHoiDongItem? KetQuaCuoiKy { get; set; }
        public KetQuaHoiDongItem? KetQuaBaoVe { get; set; } // Hội đồng bảo vệ cuối kỳ

        // Tổng hợp điểm
        // Công thức: GVHD (30%) + Hội đồng bảo vệ (70%)
        public double? DiemTrungBinhChung { get; set; }
        public string? XepLoaiTotNghiep { get; set; }

        // Trọng số hiển thị
        public const double TRONG_SO_GVHD = 0.30;
        public const double TRONG_SO_HOI_DONG = 0.70;
    }

    public class KetQuaHoiDongItem
    {
        public string? TenHoiDong { get; set; }
        public string? LoaiHoiDong { get; set; }
        public string? LoaiHoiDongText { get; set; }
        public string? DiaDiem { get; set; }
        public string? NgayBaoCao { get; set; }
        public string? ThoiGian { get; set; }
        public double? DiemTongKet { get; set; }
        public string? DiemChu { get; set; }
        public string? KetQua { get; set; }
        public string? KetQuaCss { get; set; }
        public bool DaCongBo { get; set; }
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
        public string? VaiTroCss { get; set; }
    }
}
