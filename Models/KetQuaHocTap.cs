using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class KetQuaHocTap
{
    public int Id { get; set; }

    public int? IdSinhVien { get; set; }

    public int? Stt { get; set; }

    public string? MaHocPhan { get; set; }

    public string? TenHocPhan { get; set; }

    public double? SoTc { get; set; }

    public double? DiemSo { get; set; }

    public string? DiemChu { get; set; }

    public double? TongSoTinChi { get; set; }

    public double? Gpa { get; set; }

    public bool? KetQua { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
