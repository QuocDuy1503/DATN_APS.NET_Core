using System;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class QuanLyHoiDongViewModel
    {
        public int Id { get; set; }
        public string MaHoiDong { get; set; }
        public string TenHoiDong { get; set; }
        public string TenBoMon { get; set; }
        public string NguoiTao { get; set; }
        public DateTime? NgayBaoCao { get; set; }
        public string DiaDiem { get; set; }
        public bool TrangThai { get; set; }
    }
}