using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

[Table("LichSuCapNhatDiem")]
public class LichSuCapNhatDiem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_phien_bao_ve")]
    public int IdPhienBaoVe { get; set; }

    [Column("id_sinh_vien")]
    public int IdSinhVien { get; set; }

    [Column("id_nguoi_cap_nhat")]
    public int IdNguoiCapNhat { get; set; }

    [Column("loai_cap_nhat")]
    [StringLength(50)]
    public string? LoaiCapNhat { get; set; } // THU_KY_DIEU_CHINH | CHU_TICH_XAC_NHAN

    [Column("diem_cu")]
    public double? DiemCu { get; set; }

    [Column("diem_moi")]
    public double? DiemMoi { get; set; }

    [Column("ly_do")]
    [StringLength(500)]
    public string? LyDo { get; set; }

    [Column("ngay_cap_nhat")]
    public DateTime? NgayCapNhat { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("IdPhienBaoVe")]
    public virtual PhienBaoVe? IdPhienBaoVeNavigation { get; set; }

    [ForeignKey("IdSinhVien")]
    public virtual SinhVien? IdSinhVienNavigation { get; set; }

    [ForeignKey("IdNguoiCapNhat")]
    public virtual GiangVien? IdNguoiCapNhatNavigation { get; set; }
}
