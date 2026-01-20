using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class DonPhucKhao
{
    public int Id { get; set; }

    public int? IdSinhVien { get; set; }

    public int? IdDot { get; set; }

    public string? TieuDeKhieuNai { get; set; }

    public string? NoiDungKhieuNai { get; set; }

    public string? MinhChungLink { get; set; }

    public string? TrangThai { get; set; }

    public string? PhanHoiCuaGv { get; set; }

    public DateTime? NgayGui { get; set; }

    public DateTime? NgayXuLy { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
