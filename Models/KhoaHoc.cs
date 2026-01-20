using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class KhoaHoc
{
    public int Id { get; set; }

    public string? MaKhoa { get; set; }

    public string? TenKhoa { get; set; }

    public int? NamNhapHoc { get; set; }

    public int? NamTotNghiep { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<ChuongTrinhDaoTao> ChuongTrinhDaoTaos { get; set; } = new List<ChuongTrinhDaoTao>();

    public virtual ICollection<DotDoAn> DotDoAns { get; set; } = new List<DotDoAn>();

    public virtual ICollection<SinhVien> SinhViens { get; set; } = new List<SinhVien>();
}
