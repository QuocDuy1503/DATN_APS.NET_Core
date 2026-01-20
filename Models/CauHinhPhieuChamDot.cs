using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class CauHinhPhieuChamDot
{
    public int Id { get; set; }

    public int? IdDot { get; set; }

    public string? VaiTroCham { get; set; }

    public int? IdLoaiPhieu { get; set; }

    public virtual DotDoAn? IdDotNavigation { get; set; }

    public virtual LoaiPhieuCham? IdLoaiPhieuNavigation { get; set; }
}
