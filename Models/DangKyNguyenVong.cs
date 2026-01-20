using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class DangKyNguyenVong
{
    public int Id { get; set; }

    public int? IdDot { get; set; }

    public int? IdSinhVien { get; set; }

    public int? SoTinChiTichLuyHienTai { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }
}
