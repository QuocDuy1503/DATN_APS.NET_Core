using System;
using System.Collections.Generic;

namespace DATN_TMS.Models;

public partial class DeCuong
{
    public int Id { get; set; }

    public int? IdDeTai { get; set; }

    public string? LyDoChonDeTai { get; set; }

    public string? GiaThuyetNghienCuu { get; set; }

    public string? DoiTuongNghienCuu { get; set; }

    public string? PhamViNghienCuu { get; set; }

    public string? PhuongPhapNghienCuu { get; set; }

    public DateTime? NgayNop { get; set; }

    public string? TrangThai { get; set; }

    public virtual DeTai? IdDeTaiNavigation { get; set; }
}
