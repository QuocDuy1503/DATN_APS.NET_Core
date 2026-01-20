using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class BaoCaoNop
{
    public int Id { get; set; }

    public int? IdDot { get; set; }

    public int? IdDeTai { get; set; }

    public int? IdSinhVien { get; set; }

    public int? Stt { get; set; }

    public string? TenBaoCao { get; set; }

    public string? FileBaocao { get; set; }

    public DateTime? NgayNop { get; set; }

    public string? NhanXet { get; set; }

    public string? TrangThai { get; set; }

    public virtual DeTai? IdDeTaiNavigation { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }

    public virtual ICollection<KeHoachCongViec> KeHoachCongViecs { get; set; } = new List<KeHoachCongViec>();
}
