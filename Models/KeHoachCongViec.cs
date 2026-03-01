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
    public int? IdSinhVien { get; set; }

    [Column("id_dot")]
    public int? IdDot { get; set; }

    [Column("tuan")]
    public int? Tuan { get; set; }

    [Column("thu_trong_tuan")]
    public byte? ThuTrongTuan { get; set; }

    [Column("gio_bat_dau")]
    public TimeOnly? GioBatDau { get; set; }

    [Column("gio_ket_thuc")]
    public TimeOnly? GioKetThuc { get; set; }

    [Column("gio_bat_dau_thuc_te")]
    public TimeOnly? GioBatDauThucTe { get; set; }

    [Column("gio_ket_thuc_thuc_te")]
    public TimeOnly? GioKetThucThucTe { get; set; }

    [Column("ten_cong_viec")]
    public string? TenCongViec { get; set; }

    [Column("mo_ta_cong_viec")]
    public string? MoTaCongViec { get; set; }

    [Column("trang_thai")]
    public string? TrangThai { get; set; }

    [Column("id_file_minh_chung")]
    public int? IdFileMinhChung { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual BaoCaoNop? IdFileMinhChungNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
