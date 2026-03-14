using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

[Table("XacNhanDiemChuTich")]
public class XacNhanDiemChuTich
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_phien_bao_ve")]
    public int IdPhienBaoVe { get; set; }

    [Column("id_chu_tich")]
    public int IdChuTich { get; set; }

    [Column("trang_thai")]
    [StringLength(20)]
    public string? TrangThai { get; set; } = "CHO_XAC_NHAN"; // CHO_XAC_NHAN | DA_XAC_NHAN

    [Column("diem_tong_ket_cuoi")]
    public double? DiemTongKetCuoi { get; set; }

    [Column("ghi_chu")]
    [StringLength(500)]
    public string? GhiChu { get; set; }

    [Column("ngay_xac_nhan")]
    public DateTime? NgayXacNhan { get; set; }

    // Navigation properties
    [ForeignKey("IdPhienBaoVe")]
    public virtual PhienBaoVe? IdPhienBaoVeNavigation { get; set; }

    [ForeignKey("IdChuTich")]
    public virtual GiangVien? IdChuTichNavigation { get; set; }
}
