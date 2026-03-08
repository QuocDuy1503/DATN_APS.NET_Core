using X.PagedList;

namespace DATN_TMS.Areas.SinhVien.Models
{
    // === INDEX ===
    public class KeHoachIndexViewModel
    {
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenGVHD { get; set; }
        public string? TenDot { get; set; }
        public string GiaiDoan { get; set; } = "CHUA_MO";
        public string? ThongBao { get; set; }
        public IPagedList<KeHoachItemViewModel>? DanhSachCongViec { get; set; }
    }

    public class KeHoachItemViewModel
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenCongViec { get; set; }
        public string? NguoiPhuTrach { get; set; }
        public string? ThoiGianDuKien { get; set; }
        public string? ThoiGianThucTe { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public string? StatusText { get; set; }
    }

    // === DETAIL ===
    public class KeHoachDetailViewModel
    {
        public int Id { get; set; }
        public int? Stt { get; set; }
        public string? TenCongViec { get; set; }
        public string? MoTaCongViec { get; set; }
        public string? NguoiPhuTrach { get; set; }
        public string? NgayBatDau { get; set; }
        public string? NgayKetThuc { get; set; }
        public string? NgayBatDauThucTe { get; set; }
        public string? NgayKetThucThucTe { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusText { get; set; }
        public string? StatusCss { get; set; }
        public string? GhiChu { get; set; }
        public string? TenDot { get; set; }

        // File minh chứng
        public int? IdFileMinhChung { get; set; }
        public string? TenFileMinhChung { get; set; }
        public string? LinkFileMinhChung { get; set; }

        // Nhận xét GV
        public string? NhanXetGiangVien { get; set; }

        // Gợi ý SV trong đề tài
        public List<SinhVienGợiYItem> DanhSachSinhVienDeTai { get; set; } = new();

        public bool IsEditable { get; set; }
    }

    // === CREATE ===
    public class KeHoachCreateViewModel
    {
        public List<SinhVienGợiYItem> DanhSachSinhVienDeTai { get; set; } = new();
    }

    public class SinhVienGợiYItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
    }
}
