namespace DATN_TMS.Areas.SinhVien.Models
{
    public class DangKyDeTaiViewModel
    {
        public int IdDeTai { get; set; }
        public string? MaDeTai { get; set; }
        public string? TenDeTai { get; set; }
        public string? Nganh { get; set; }
        public string? GVHD { get; set; }
        public string? TrangThai { get; set; }
        public string? StatusCss { get; set; }
        public bool DaDangKy { get; set; }

        // BUSINESS RULE #3: Đánh dấu đề tài do SV tự đề xuất
        public bool LaDeTaiSVTuDeXuat { get; set; }

        // Trạng thái sĩ số: Số đã duyệt / Tối đa
        public int SoSinhVienDaDuyet { get; set; }
        public int SoSlotToiDa { get; set; } = 2;
        public string TrangThaiSiSo => $"{SoSinhVienDaDuyet}/{SoSlotToiDa}";
        public bool DaDuNguoi => SoSinhVienDaDuyet >= SoSlotToiDa;

        // Trạng thái đăng ký của SV hiện tại với đề tài này
        // "Đã duyệt" | "Đã đăng ký" | null (trống)
        public string? TrangThaiDangKyCuaSV { get; set; }
        public bool SvDaDuyetDeTaiNay { get; set; } // SV đã được duyệt vào chính đề tài này
        public bool SvDaDangKyDeTaiNay { get; set; } // SV đã đăng ký (chờ duyệt) đề tài này
    }
}
