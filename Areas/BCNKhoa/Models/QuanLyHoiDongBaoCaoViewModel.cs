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

        // Trạng thái duyệt: CHO_DUYET, DA_DUYET, TU_CHOI
        public string TrangThaiDuyet { get; set; } = "CHO_DUYET";

        public string TrangThaiDuyetDisplay => TrangThaiDuyet switch
        {
            "DA_DUYET" => "Đã duyệt",
            "TU_CHOI" => "Từ chối",
            _ => "Chờ duyệt"
        };

        public string TrangThaiDuyetBadgeClass => TrangThaiDuyet switch
        {
            "DA_DUYET" => "bg-success",
            "TU_CHOI" => "bg-danger",
            _ => "bg-warning text-dark"
        };

        public string LoaiHoiDongDisplay => LoaiHoiDong switch
        {
            "GIUA_KY" => "Giữa kỳ",
            "CUOI_KY" => "Cuối kỳ",
            _ => LoaiHoiDong
        };
    }
}