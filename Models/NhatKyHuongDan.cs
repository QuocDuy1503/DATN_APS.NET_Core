using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

[Table("NhatKyHuongDan")]
public partial class NhatKyHuongDan
{
    [Column("id")]
    public int Id { get; set; }

    [Column("id_dot")]
    public int? IdDot { get; set; }

    [Column("ngay_hop")]
    public DateOnly? NgayHop { get; set; }

    [Column("thoi_gian_hop")]
    public TimeOnly? ThoiGianHop { get; set; }

    [Column("hinh_thuc_hop")]
    public string? HinhThucHop { get; set; }

    [Column("dia_diem_hop")]
    public string? DiaDiemHop { get; set; }

    [Column("thanh_vien_tham_du")]
    public string? ThanhVienThamDu { get; set; }

    [Column("ten_gvhd")]
    public string? TenGvhd { get; set; }

    [Column("muc_tieu_buoi_hop")]
    public string? MucTieuBuoiHop { get; set; }

    [Column("noi_dung_hop")]
    public string? NoiDungHop { get; set; }

    [Column("action_list")]
    public string? ActionList { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }
}
