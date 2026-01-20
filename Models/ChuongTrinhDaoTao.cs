using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class ChuongTrinhDaoTao
{
    public int Id { get; set; }

    public string MaCtdt { get; set; } = null!;

    public string? TenCtdt { get; set; }

    public int? SttHienThi { get; set; }

    public int? IdNganh { get; set; }

    public int? IdKhoaHoc { get; set; }

    public int? TongTinChi { get; set; }

    public bool? TrangThai { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ChiTietCtdt> ChiTietCtdts { get; set; } = new List<ChiTietCtdt>();

    public virtual KhoaHoc? IdKhoaHocNavigation { get; set; }

    public virtual Nganh? IdNganhNavigation { get; set; }
}
