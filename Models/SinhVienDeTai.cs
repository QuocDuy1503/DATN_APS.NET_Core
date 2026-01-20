using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class SinhVienDeTai
{
    public int Id { get; set; }

    public int? IdDeTai { get; set; }

    public int? IdSinhVien { get; set; }

    public string? TrangThai { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public string? NhanXet { get; set; }

    public virtual DeTai? IdDeTaiNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }

    public virtual ICollection<PhienBaoVe> PhienBaoVes { get; set; } = new List<PhienBaoVe>();
}
