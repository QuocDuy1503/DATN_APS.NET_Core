using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class ThongBao
{
    public int Id { get; set; }

    public int? IdNguoiNhan { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public string? LinkLienKet { get; set; }

    public bool? TrangThaiXem { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung? IdNguoiNhanNavigation { get; set; }
}
