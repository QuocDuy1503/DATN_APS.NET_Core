using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class NguoiDung
{
    public int Id { get; set; }

    public string? HoTen { get; set; }

    public string Email { get; set; } = null!;

    public string? MatKhau { get; set; }

    public string? Sdt { get; set; }

    public string? MicrosoftId { get; set; }

    public string? AvatarUrl { get; set; }

    public int? TrangThai { get; set; }

    public virtual ICollection<DeTai> DeTais { get; set; } = new List<DeTai>();

    public virtual GiangVien? GiangVien { get; set; }

    public virtual ICollection<LoaiPhieuCham> LoaiPhieuChams { get; set; } = new List<LoaiPhieuCham>();

    public virtual SinhVien? SinhVien { get; set; }

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();

    public virtual ICollection<VaiTro> IdVaiTros { get; set; } = new List<VaiTro>();
}
