namespace DATN_TMS.Models
{
    public class HoiDongViewModel
    {
        public int Id { get; set; }
        public string? TenHoiDong { get; set; }
        public string? BoMon { get; set; }
        public string? LoaiHoiDong { get; set; }
        public string? NguoiTao { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string? DiaDiem { get; set; }
        public string? TrangThai { get; set; }
    }
}
