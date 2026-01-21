using System;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class QuanLyHoiDongBaoCaoViewModel
    {
        public int Id { get; set; }
        public string MaHoiDong { get; set; } = string.Empty;
        public string TenHoiDong { get; set; } = string.Empty;

        // Loại hội đồng: "Giữa kì" hoặc "Cuối kì"
        public string LoaiHoiDong { get; set; } = string.Empty;

        public int IdBoMon { get; set; }
        public string? TenBoMon { get; set; } = string.Empty;
        public string? NguoiTao { get; set; } = string.Empty;

        // Cập nhật đúng kiểu dữ liệu của Entity
        public DateOnly? NgayBaoCao { get; set; }

        // Mapping từ cột ThoiGianDuKien trong DB
        public TimeOnly? ThoiGianDuKien { get; set; }

        public string? DiaDiem { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
        public TimeOnly? GioBatDau { get; internal set; }
    }
}