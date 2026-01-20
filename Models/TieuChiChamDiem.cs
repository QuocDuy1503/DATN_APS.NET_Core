using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class TieuChiChamDiem
{
    public int Id { get; set; }

    public int? IdLoaiPhieu { get; set; }

    public string? TenTieuChi { get; set; }

    public string? MoTaHuongDan { get; set; }

    public double? TrongSo { get; set; }

    public double? DiemToiDa { get; set; }

    public int? SttHienThi { get; set; }

    public virtual ICollection<DiemChiTiet> DiemChiTiets { get; set; } = new List<DiemChiTiet>();

    public virtual LoaiPhieuCham? IdLoaiPhieuNavigation { get; set; }
}
