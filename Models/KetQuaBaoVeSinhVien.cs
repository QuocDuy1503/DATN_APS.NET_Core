using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class KetQuaBaoVeSinhVien
{
    public int Id { get; set; }

    public int? IdPhienBaoVe { get; set; }

    public int? IdSinhVien { get; set; }

    public double? DiemTongKet { get; set; }

    public string? DiemChu { get; set; }

    public string? KetQua { get; set; }

    public virtual PhienBaoVe? IdPhienBaoVeNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
