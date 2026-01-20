using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class DiemChiTiet
{
    public int Id { get; set; }

    public int? IdPhienBaoVe { get; set; }

    public int? IdNguoiCham { get; set; }

    public int? IdSinhVien { get; set; }

    public int? IdTieuChi { get; set; }

    public double? DiemSo { get; set; }

    public string? NhanXet { get; set; }

    public virtual GiangVien? IdNguoiChamNavigation { get; set; }

    public virtual PhienBaoVe? IdPhienBaoVeNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }

    public virtual TieuChiChamDiem? IdTieuChiNavigation { get; set; }
}
