using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

[Table("BaoCaoThongKe")]
public partial class BaoCaoThongKe
{
    [Column("id")]
    public int Id { get; set; }

    [Column("id_dot")]
    public int IdDot { get; set; }

    [Column("so_luong_sinh_vien")]
    public int SoLuongSinhVien { get; set; }

    [Column("so_luong_de_tai")]
    public int SoLuongDeTai { get; set; }

    [Column("so_luong_task_tuan")]
    public int? SoLuongTaskTuan { get; set; }

    [Column("ti_le_hoan_thanh")]
    public double? TiLeHoanThanh { get; set; }

    [Column("ngay_tinh")]
    public DateTime NgayTinh { get; set; }

    [Column("chi_tiet_tuan")]
    public string? ChiTietTuan { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }
}
