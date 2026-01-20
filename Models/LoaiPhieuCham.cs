using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class LoaiPhieuCham
{
    public int Id { get; set; }

    public string? TenLoaiPhieu { get; set; }

    public int? NguoiTao { get; set; }

    public virtual ICollection<CauHinhPhieuChamDot> CauHinhPhieuChamDots { get; set; } = new List<CauHinhPhieuChamDot>();

    public virtual NguoiDung? NguoiTaoNavigation { get; set; }

    public virtual ICollection<TieuChiChamDiem> TieuChiChamDiems { get; set; } = new List<TieuChiChamDiem>();
}
