using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN_TMS.Models;

[Table("ThanhVien_HD_BaoCao")]
public partial class ThanhVienHdBaoCao
{
    [Column("id")]
    public int Id { get; set; }
    [Column("id_hd_baocao")]
    public int? IdHdBaocao { get; set; }
    [Column("id_giang_vien")]
    public int? IdGiangVien { get; set; }
    [Column("vai_tro")]
    public string? VaiTro { get; set; }

    public virtual GiangVien? IdGiangVienNavigation { get; set; }

    public virtual HoiDongBaoCao? IdHdBaocaoNavigation { get; set; }
}
