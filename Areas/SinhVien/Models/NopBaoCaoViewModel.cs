namespace DATN_TMS.Areas.SinhVien.Models
{
    public class NopBaoCaoIndexViewModel
    {
        public string? TenDot { get; set; }
        public string? HocKi { get; set; }
        public string? ThongBaoDot { get; set; }
        public bool CoDot { get; set; }
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

        // Trạng thái giai đoạn: CHUA_MO, DANG_MO, DA_DONG
        public string TrangThaiGiaiDoan { get; set; } = "CHUA_MO";
        public string TrangThaiGiaiDoanText { get; set; } = "Chưa mở";
        public string TrangThaiGiaiDoanCss { get; set; } = "phase-pending";

        public bool DangMo { get; set; }
    }

    public class NopBaoCaoDetailViewModel
    {
        public int? IdBaoCaoNop { get; set; }
        public string LoaiBaoCao { get; set; } = "";
        public string TieuDe { get; set; } = "";
        public string? TenDot { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? TenGVHD { get; set; }

        public string? TrangThaiGui { get; set; }
        public string? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TrangThaiCss { get; set; }

        // Trạng thái giai đoạn
        public string TrangThaiGiaiDoan { get; set; } = "CHUA_MO";
        public string TrangThaiGiaiDoanText { get; set; } = "Chưa mở";
        public string TrangThaiGiaiDoanCss { get; set; } = "phase-pending";

        public string? TenFile { get; set; }
        public string? FilePath { get; set; }
        public string? NgayNop { get; set; }
        public string? NgaySuaDoiCuoi { get; set; }
        public string? KichThuocFile { get; set; }

        // Thông tin người nộp (đại diện nhóm)
        public string? NguoiNop { get; set; }
        public bool LaNguoiDaiDien { get; set; }

        public string? GhiChuGui { get; set; }
        public string? NhanXetGVHD { get; set; }
        public string? NgayNhanXet { get; set; }

        public string? ThoiGianBatDau { get; set; }
        public string? ThoiGianKetThuc { get; set; }
        public int? SoNgayConLai { get; set; }
        public bool DangMo { get; set; }
        public bool SapHetHan { get; set; } // Còn <= 2 ngày

        // Kiểm tra có thể nộp hoặc xóa
        public bool CanUpload { get; set; }
        public bool CanDelete { get; set; }
        public string? LyDoKhongTheNop { get; set; }

        // Yêu cầu báo cáo
        public string? YeuCauBaoCao { get; set; }

        public int? IdDot { get; set; }
        public int? IdDeTai { get; set; }
        public int? IdSinhVien { get; set; }

        // Danh sách sinh viên cùng đề tài (để xử lý nhóm)
        public List<SinhVienNhomItem> DanhSachNhom { get; set; } = new();

        // Lịch sử nộp
        public List<LichSuNopItem> LichSuNop { get; set; } = new();
    }

    public class SinhVienNhomItem
    {
        public int IdSinhVien { get; set; }
        public string? Mssv { get; set; }
        public string? HoTen { get; set; }
    }

    public class LichSuNopItem
    {
        public string? TenFile { get; set; }
        public string? NgayNop { get; set; }
        public string? NguoiNop { get; set; }
        public string? TrangThai { get; set; }
        public string? TrangThaiText { get; set; }
        public string? TrangThaiCss { get; set; }
    }
}
