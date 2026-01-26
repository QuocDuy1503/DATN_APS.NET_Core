using System;

namespace DATN_TMS.Areas.BCNKhoa.Models
{
    public class ChuyenNganhViewModel
    {
        public int Id { get; set; }

        public int? Stt { get; set; }

        public string TenChuyenNganh { get; set; }

        public string TenVietTat { get; set; }

        public string NguoiTao { get; set; }

        public DateOnly? NgayTao { get; set; }

        public string NguoiSua { get; set; }

        public DateOnly? NgaySua { get; set; }

        public string TenNganh { get; set; }
        public string TenBoMon { get; set; }
    }
}