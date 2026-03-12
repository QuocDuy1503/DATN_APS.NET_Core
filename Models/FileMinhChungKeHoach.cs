using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

/// <summary>
/// Bảng lưu trữ nhiều file minh chứng cho một kế hoạch công việc
/// </summary>
[Table("FileMinhChung_KeHoach")]
public class FileMinhChungKeHoach
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_ke_hoach")]
    public int IdKeHoach { get; set; }

    [Column("id_sinh_vien")]
    public int? IdSinhVien { get; set; }

    [Column("ten_file")]
    [StringLength(255)]
    public string? TenFile { get; set; }

    [Column("duong_dan")]
    [StringLength(500)]
    public string? DuongDan { get; set; }

    [Column("loai_file")]
    [StringLength(20)]
    public string? LoaiFile { get; set; } // PDF, IMAGE

    [Column("ngay_nop")]
    public DateTime? NgayNop { get; set; }

    // Navigation properties
    [ForeignKey("IdKeHoach")]
    public virtual KeHoachCongViec? IdKeHoachNavigation { get; set; }

    [ForeignKey("IdSinhVien")]
    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
