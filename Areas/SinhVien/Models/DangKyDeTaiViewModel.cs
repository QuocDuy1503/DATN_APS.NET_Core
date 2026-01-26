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
    }
}
