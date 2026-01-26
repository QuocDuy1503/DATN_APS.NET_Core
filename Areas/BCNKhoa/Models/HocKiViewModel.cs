using System;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class HocKiViewModel
    {
        public int Id { get; set; }
        public string MaHocKi { get; set; } 
        public int? NamBatDau { get; set; }
        public int? NamKetThuc { get; set; }
        public int? TuanBatDau { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public bool? TrangThai { get; set; } 
    }
}