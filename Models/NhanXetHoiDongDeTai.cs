using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models
{
    [Table("NhanXetHoiDongDeTai")]
    public class NhanXetHoiDongDeTai
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_de_tai")]
        public int IdDeTai { get; set; }

        [Column("id_giang_vien")]
        public int IdGiangVien { get; set; }

        [Column("trang_thai")]
        public string? TrangThai { get; set; }

        [Column("nhan_xet")]
        public string? NhanXet { get; set; }

        [Column("ngay_tao")]
        public DateTime NgayTao { get; set; }

        public virtual DeTai? DeTai { get; set; }

        public virtual GiangVien? GiangVien { get; set; }
    }
}
