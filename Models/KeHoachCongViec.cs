using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

[Table("KeHoachCongViec")]
public partial class KeHoachCongViec
{
    [Column("id")]
    public int Id { get; set; }

    [Column("stt")]
    public int? Stt { get; set; }

    [Column("id_sinh_vien")]
    public int IdSinhVien { get; set; }

    [Column("id_dot")]
    public int? IdDot { get; set; }

    [Column("ngay_bat_dau")]
    public DateOnly? NgayBatDau { get; set; }

    [Column("ngay_ket_thuc")]
    public DateOnly? NgayKetThuc { get; set; }

    [Column("ngay_bat_dau_thuc_te")]
    public DateOnly? NgayBatDauThucTe { get; set; }

    [Column("ngay_ket_thuc_thuc_te")]
    public DateOnly? NgayKetThucThucTe { get; set; }

    [Column("ten_cong_viec")]
    public string? TenCongViec { get; set; }

    [Column("mo_ta_cong_viec")]
    public string? MoTaCongViec { get; set; }

    [Column("trang_thai")]
    public string? TrangThai { get; set; }

    [Column("ghi_chu")]
    public string? GhiChu { get; set; }

    [Column("id_file_minh_chung")]
    public int? IdFileMinhChung { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual BaoCaoNop? IdFileMinhChungNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
