//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//namespace DATN_TMS.Models;

//public partial class HoiDongBaoCao
//{
//    public int Id { get; set; }

//    public string? MaHoiDong { get; set; }

//    public string? TenHoiDong { get; set; }

//    public string? LoaiHoiDong { get; set; }

//    public int? IdDot { get; set; }

//    public int? IdNguoiTao { get; set; }

//    public int? IdBoMon { get; set; }

//    public DateOnly? NgayBaoCao { get; set; }
//    [Column("ngay_bat_dau")]
//    public DateOnly? NgayBatDau { get; set; }

//    [Column("ngay_ket_thuc")] 
//    public DateOnly? NgayKetThuc { get; set; }

//    public string? DiaDiem { get; set; }

//    public TimeOnly? ThoiGianDuKien { get; set; }

//    public bool? TrangThai { get; set; }

//    public virtual BoMon? IdBoMonNavigation { get; set; }

//    public virtual DotDoAn? IdDotNavigation { get; set; }
//    [ForeignKey("IdNguoiTao")]
//    public virtual NguoiDung? IdNguoiTaoNavigation { get; set; }

//    public virtual ICollection<PhienBaoVe> PhienBaoVes { get; set; } = new List<PhienBaoVe>();

//    public virtual ICollection<ThanhVienHdBaoCao> ThanhVienHdBaoCaos { get; set; } = new List<ThanhVienHdBaoCao>();

//}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models
{
    [Table("HoiDongBaoCao")]
    public partial class HoiDongBaoCao
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("ma_hoi_dong")]
        public string? MaHoiDong { get; set; }

        [Column("ten_hoi_dong")]
        public string? TenHoiDong { get; set; }

        [Column("loai_hoi_dong")]
        public string? LoaiHoiDong { get; set; }

        [Column("id_dot")]
        public int? IdDot { get; set; }

        [Column("id_nguoi_tao")]
        public int? IdNguoiTao { get; set; }

        [Column("id_bo_mon")]
        public int? IdBoMon { get; set; }

        [Column("ngay_bao_cao")]
        public DateOnly? NgayBaoCao { get; set; }

        [Column("ngay_bat_dau")]
        public DateOnly? NgayBatDau { get; set; }

        [Column("ngay_ket_thuc")]
        public DateOnly? NgayKetThuc { get; set; }

        [Column("dia_diem")]
        public string? DiaDiem { get; set; }

        [Column("thoi_gian_du_kien")]
        public TimeOnly? ThoiGianDuKien { get; set; }

        [Column("trang_thai")]
        public bool? TrangThai { get; set; }

        public virtual BoMon? IdBoMonNavigation { get; set; }
        public virtual DotDoAn? IdDotNavigation { get; set; }
        [ForeignKey("IdNguoiTao")]
        public virtual NguoiDung? IdNguoiTaoNavigation { get; set; }
        public virtual ICollection<PhienBaoVe> PhienBaoVes { get; set; } = new List<PhienBaoVe>();
        public virtual ICollection<ThanhVienHdBaoCao> ThanhVienHdBaoCaos { get; set; } = new List<ThanhVienHdBaoCao>();
    }
}