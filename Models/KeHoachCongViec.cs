using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class KeHoachCongViec
{
    public int Id { get; set; }

    public int? Stt { get; set; }

    public int? IdSinhVien { get; set; }

    public string? TenCongViec { get; set; }

    public string? MoTaCongViec { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public DateTime? NgayBatDauThucTe { get; set; }

    public DateTime? NgayKetThucThucTe { get; set; }

    public string? TrangThai { get; set; }

    public int? IdFileMinhChung { get; set; }

    public virtual BaoCaoNop? IdFileMinhChungNavigation { get; set; }

    public virtual SinhVien? IdSinhVienNavigation { get; set; }

    public virtual ICollection<NhatKyHuongDan> NhatKyHuongDans { get; set; } = new List<NhatKyHuongDan>();
}
