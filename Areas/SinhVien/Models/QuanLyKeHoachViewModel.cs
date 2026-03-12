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
        public string? TrangThaiDeTai { get; set; } // Trạng thái đề tài (CHO_DUYET, DA_DUYET, TU_CHOI)
        public IPagedList<KeHoachItemViewModel>? DanhSachCongViec { get; set; }

        // Danh sách sinh viên cùng đề tài để chọn người phụ trách
        public List<SinhVienGợiYItem> DanhSachSinhVienDeTai { get; set; } = new();
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

        // Danh sách file minh chứng (hỗ trợ nhiều file)
        public List<FileMinhChungItem> DanhSachFileMinhChung { get; set; } = new();

        // Nhận xét GV
        public string? NhanXetGiangVien { get; set; }

        // Gợi ý SV trong đề tài
        public List<SinhVienGợiYItem> DanhSachSinhVienDeTai { get; set; } = new();

        public bool IsEditable { get; set; }

        // Kiểm tra có file minh chứng hay không
        public bool HasFileMinhChung => DanhSachFileMinhChung.Any();
    }

    public class FileMinhChungItem
    {
        public int Id { get; set; }
        public string? TenFile { get; set; }
        public string? LinkFile { get; set; }
        public string? LoaiFile { get; set; } // PDF, IMAGE
        public DateTime? NgayNop { get; set; }
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
